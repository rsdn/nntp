using System;
using derIgel.NNTP.ru.rsdn;
using System.Reflection;
using System.IO;
using derIgel.NNTP;
using System.Net;
using derIgel.Mail;
using System.Text.RegularExpressions;
using System.Configuration;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// 
		/// </summary>
		public class RsdnDataProvider : derIgel.NNTP.DataProvider
		{
			/// <summary>
			/// read cache at the start
			/// </summary>
			static RsdnDataProvider()
			{
				cacheFilename = Assembly.GetExecutingAssembly().GetName().Name + ".cache";
				if (File.Exists(cacheFilename))
					cache = Cache.Deserialize(cacheFilename);
				else
					cache = new Cache();
			}

			/// <summary>
			/// Write cache at the end
			/// </summary>
			~RsdnDataProvider()
			{
				cache.Serialize(cacheFilename);
			}

			public RsdnDataProvider(NNTPSettings settings) : base(settings)
			{
				PostingAllowed = true;
				webService = new Forum();
				encoding = System.Text.Encoding.UTF8;
				RsdnNntpSettings rsdnSettings = settings as RsdnNntpSettings;
				if (rsdnSettings != null)
				{
					webService.Url = ((RsdnNntpSettings)rsdnSettings).Service;
					webService.Proxy = rsdnSettings.GetProxy;
					encoding = rsdnSettings.GetEncoding;
					cache.Capacity = rsdnSettings.CacheSize;
				}
				Stream io = Assembly.GetExecutingAssembly().GetManifestResourceStream("derIgel.NNTP.Header.htm");
				StreamReader reader = new StreamReader(io);
				htmlMessageTemplate = reader.ReadToEnd();
				reader.Close();
			}

			protected Forum webService;

			public override NewsGroup GetGroup(string groupName)
			{
				group requestedGroup = null;
				try
				{
					requestedGroup = webService.GroupInfo(groupName, username, password);
				}		
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					ProcessSoapException(exception);
				}
				currentGroup = groupName;
				currentGroupArticleStartNumber = requestedGroup.first;
				currentGroupArticleEndNumber = requestedGroup.last;
				return new NewsGroup(groupName,	requestedGroup.first, requestedGroup.last,
					requestedGroup.last - requestedGroup.first + 1, true);
			}

			public override NewsArticle GetArticle(int articleNumber, NewsArticle.Content content)
			{
				if (currentGroup == null)
					throw new Exception(Errors.NoSelectedGroup);

				NewsArticle newsMessage = null;
				// synchronized access to cache
				if (cache.Capacity > 0)
					lock(cache)
					{
						newsMessage = cache[new Cache.NewsArticleIdentity(null, currentGroup, articleNumber)];
					}

				if (newsMessage == null)
				{
					article message = null;
					try
					{
						message = webService.GetFormattedArticle(currentGroup, articleNumber,
							username,	password);
					}
					catch (System.Web.Services.Protocols.SoapException exception)
					{
						ProcessSoapException(exception);
					}	

					if (message.error != null)
						switch (Convert.ToInt32(message.error.Split(new char[]{'\t', ' '}, 2)[0]))
						{
							case 3:
								throw new DataProvider.Exception(DataProvider.Errors.NoSuchArticle);
								//break;
							default:
								throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
								//break;
						}

					currentArticle = articleNumber;
					newsMessage = ToNNTPArticle(message, currentGroup, content);
					// synchronized access to cache
					if (cache.Capacity > 0)
						lock(cache)
						{
							cache[new Cache.NewsArticleIdentity(newsMessage.MessageID, currentGroup, articleNumber)] =
								newsMessage;
						}					
				}
				return newsMessage;
			}

			public override NewsArticle GetArticle(string messageID, NewsArticle.Content content)
			{
				// web service doesn't support this feature
				throw new Exception(Errors.NotSupported);

			}

			public override NewsArticle GetNextArticle()
			{
				if (currentGroup == null)
					throw new Exception(Errors.NoSelectedGroup);

				if (currentArticle == -1)
					throw new Exception(Errors.NoSelectedArticle);

				NewsArticle[] articleList = GetArticleList(currentArticle + 1, currentGroupArticleEndNumber,
					NewsArticle.Content.Header);
		
				if (articleList.Length == 0)
					throw new Exception(Errors.NoNextArticle);

				currentArticle = articleList[0].Number;

				return articleList[0];
			}

			public override NewsArticle GetPrevArticle()
			{
				if (currentGroup == null)
					throw new Exception(Errors.NoSelectedGroup);

				if (currentArticle == -1)
					throw new Exception(Errors.NoSelectedArticle);

				NewsArticle[] articleList = GetArticleList(currentGroupArticleStartNumber,
					currentArticle - 1,	NewsArticle.Content.Header);
		
				if (articleList.Length == 0)
					throw new Exception(Errors.NoPrevArticle);

				currentArticle = articleList[articleList.Length - 1].Number;
				return articleList[articleList.Length - 1];
			}

			public override NewsGroup[] GetGroupList(DateTime startDate, string[] distributions)
			{
				// minimum date, supported by web service, is unknown...
				// So take midnight of 30 december 1899
				DateTime minDate = new DateTime(1899, 12, 30, 0, 0, 0, 0);
				if (startDate < minDate)
					startDate = minDate; 

				group_list groupList = null;
				try
				{
					groupList = webService.GetGroupList(username, password, startDate);
				}				
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					ProcessSoapException(exception);
				}	
				NewsGroup[] listOfGroups = new NewsGroup[groupList.groups.GetLength(0)];
				for (int i = 0; i < groupList.groups.GetLength(0); i++)
					listOfGroups[i] = new NewsGroup(groupList.groups[i].name, groupList.groups[i].first,
						groupList.groups[i].last, groupList.groups[i].last - groupList.groups[i].first + 1,
						true);
				return listOfGroups;
			}

			public override NewsArticle[] GetArticleList(string[] newsgroups, System.DateTime date,
				string[] distributions)
			{
				throw new Exception(Errors.NotSupported);
			}

			public override bool Authentificate(string user, string pass)
			{
				auth_info auth = null;
				try
				{
					auth = webService.Authentication(user, pass);
				}
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					ProcessSoapException(exception);
				}
				return auth.ok;
			}

			protected NewsArticle ToNNTPArticle(article message, string newsgroup, NewsArticle.Content content)
			{
				NewsArticle newsMessage = new NewsArticle("<" + message.id + message.postfix + ">",
					message.num);
				newsMessage.Encoding = encoding;

				if ((content == NewsArticle.Content.Header) ||
					(content == NewsArticle.Content.HeaderAndBody))
				{
					newsMessage["From"] = string.Format("{0} <{1}>", message.author, null);
					newsMessage["Date"] = message.date.ToUniversalTime().ToString("r");
					newsMessage["Subject"] = message.subject;

					if (message.pid != string.Empty)
						newsMessage["References"] = "<" + message.pid + message.postfix + ">";
					newsMessage["Newsgroups"] = newsgroup;
				}

				if ((content == NewsArticle.Content.Body) ||
					(content == NewsArticle.Content.HeaderAndBody))
				{
					Message htmlTextBody = new Message();
					htmlTextBody.Bodies.Add(string.Format(htmlMessageTemplate, message.authorid, message.author,
						message.gid, message.id, message.fmtmessage));
					htmlTextBody.Encoding = encoding;
					htmlTextBody.BodyEncoding = Message.BodyEncodingEnum.Base64;
					htmlTextBody.ContentType = "text/html";

					Message plainTextBody = new Message();
					plainTextBody.Bodies.Add(message.message);
					plainTextBody.Encoding = encoding;
					plainTextBody.BodyEncoding = Message.BodyEncodingEnum.Base64;
					plainTextBody.ContentType = "text/plain";

					newsMessage.Bodies.Add(plainTextBody);
					newsMessage.Bodies.Add(htmlTextBody);
					newsMessage.ContentType = "multipart/alternative";
				}
		
				return newsMessage;
			}

			public override NNTP.NewsArticle[] GetArticleList(int startNumber, int endNumber,
				NNTP.NewsArticle.Content content)
			{
				if (this.currentGroup == null)
					throw new Exception(Errors.NoSelectedGroup);

				article_list articleList = null;
				try
				{
					articleList = webService.ArticleList(currentGroup,
						(startNumber == -1) ? currentGroupArticleStartNumber : startNumber,
						(endNumber == -1) ? currentGroupArticleEndNumber : endNumber, username, password);
				}
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					ProcessSoapException(exception);
				}	

				NewsArticle[] articleArray = new NewsArticle[articleList.articles.GetLength(0)];

				for (int i = 0; i <articleList.articles.GetLength(0); i++)
					articleArray[i] =
						ToNNTPArticle(articleList.articles[i], currentGroup, content);

				return articleArray;
			}

			/// <summary>
			/// start article number for current group
			/// </summary>
			protected int currentGroupArticleStartNumber = -1;
			/// <summary>
			/// end article number for current group
			/// </summary>
			protected int currentGroupArticleEndNumber = -1;

			public override void PostMessage(string text)
			{
				try
				{
					post_result result = webService.PostMIMEMessage(username, password, text);
					if (!result.ok)
						throw new Exception(Errors.PostingFailed);
				}
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					ProcessSoapException(exception);
				}	
			}

			protected readonly string htmlMessageTemplate;
			protected System.Text.Encoding encoding;
			
			/// <summary>
			/// Cache
			/// </summary>
			static protected Cache cache;
			/// <summary>
			/// Cache filename
			/// </summary>
			static string cacheFilename;

			protected void ProcessSoapException(System.Web.Services.Protocols.SoapException exception)
			{
				switch (Regex.Match(exception.Message, @"^.+ ---> .+: (?<error>.+)").Groups["error"].Value)
				{
					case "1 Incorrect group name":
						throw new Exception(Errors.NoSuchGroup);
					case "2 Incorrect login name or password":
						throw new Exception(Errors.NoPermission);
					default:
						throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}
			}
		}
	}
}