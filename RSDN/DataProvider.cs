using System;
using derIgel.NNTP.ru.rsdn;
using System.Reflection;
using System.IO;
using derIgel.NNTP;
using System.Net;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// 
		/// </summary>
		public class RsdnDataProvider : derIgel.NNTP.DataProvider
		{
			public RsdnDataProvider(NNTPSettings settings) : base(settings)
			{
				webService = new Forum();
				encoding = System.Text.Encoding.UTF8;
				RsdnNntpSettings rsdnSettings = settings as RsdnNntpSettings;
				if (rsdnSettings != null)
				{
					webService.Url = ((RsdnNntpSettings)rsdnSettings).Service;
					webService.Proxy = rsdnSettings.GetProxy;
					encoding = rsdnSettings.GetEncoding;
				}
					
				startNumber = -1;
				endNumber = -1;
				Stream io = Assembly.GetExecutingAssembly().GetManifestResourceStream("derIgel.NNTP.Header.htm");
				StreamReader reader = new StreamReader(io);
				htmlMessageTemplate = reader.ReadToEnd();
				reader.Close();
			}

			protected Forum webService;

			public override NewsGroup GetGroup(string groupName)
			{
				try
				{
					group requestedGroup = webService.GroupInfo(groupName, username, password);
					currentGroup = groupName;
					startNumber = requestedGroup.first;
					endNumber = requestedGroup.last;
					return new NewsGroup(groupName,	requestedGroup.first, requestedGroup.last,
						requestedGroup.last - requestedGroup.first + 1, true);
				}		
				catch (System.Web.Services.Protocols.SoapException exception)
				{
					if (exception.Message.IndexOf("1 Incorrect group name") > 0)
						throw new Exception(Errors.NoSuchGroup);
					else
						throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}
			}

			public override NewsArticle GetArticle(int articleNumber, NewsArticle.Content content)
			{
				try
				{
					if (currentGroup == null)
						throw new Exception(Errors.NoSelectedGroup);

					article message = webService.GetFormattedArticle(currentGroup, articleNumber,
						username,	password);

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

					return ToNNTPArticle(message, currentGroup, content);
				}
				catch (System.Web.Services.Protocols.SoapException)
				{
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}	
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

				NewsArticle[] articleList = GetArticleList(currentArticle + 1, endNumber, NewsArticle.Content.Header);
		
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

				NewsArticle[] articleList = GetArticleList(startNumber, currentArticle - 1, NewsArticle.Content.Header);
		
				if (articleList.Length == 0)
					throw new Exception(Errors.NoPrevArticle);

				currentArticle = articleList[articleList.Length - 1].Number;
				return articleList[articleList.Length - 1];
			}

			public override NewsGroup[] GetGroupList(DateTime startDate, string[] distributions)
			{
				try
				{
					// minimum date, supported by web service, is unknown...
					// So take midnight of 30 december 1899
					DateTime minDate = new DateTime(1899, 12, 30, 0, 0, 0, 0);
					if (startDate < minDate)
						startDate = minDate; 

					group_list groupList;
					groupList = webService.GetGroupList(username, password, startDate);
					NewsGroup[] listOfGroups = new NewsGroup[groupList.groups.GetLength(0)];
					for (int i = 0; i < groupList.groups.GetLength(0); i++)
						listOfGroups[i] = new NewsGroup(groupList.groups[i].name, groupList.groups[i].first,
							groupList.groups[i].last, groupList.groups[i].last - groupList.groups[i].first + 1,
							false);
					return listOfGroups;
				}
				catch (System.Web.Services.Protocols.SoapException)
				{
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}	

			}

			public override NewsArticle[] GetArticleList(string[] newsgroups, System.DateTime date, string[] distributions)
			{
				return new NewsArticle[0];
			}

			public override bool Authentificate(string user, string pass)
			{
				try
				{
					auth_info auth = webService.Authentication(user, pass);
					return auth.ok;
				}
				catch (System.Web.Services.Protocols.SoapException)
				{
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}
			}

			protected NewsArticle ToNNTPArticle(article message, string newsgroup, NewsArticle.Content content)
			{
				System.Collections.Hashtable header = null;
				string body = null;

				if ((content == NewsArticle.Content.Header) ||
					(content == NewsArticle.Content.HeaderAndBody))
				{
					header = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
					header["From"] = string.Format("{0} <{1}>", message.author, null);
					header["Date"] = message.date.ToString("r");
					header["Subject"] = message.subject;

					if (message.pid != string.Empty)
						header["References"] = "<" + message.pid + message.postfix + ">";
					header["Newsgroups"] = newsgroup;
				}

				if ((content == NewsArticle.Content.Body) ||
					(content == NewsArticle.Content.HeaderAndBody))
					body = string.Format(htmlMessageTemplate, message.authorid, message.author,
						message.gid, message.id, message.fmtmessage);
		
				return new NewsArticle("<" + message.id + message.postfix + ">",
					message.num, header, body, encoding);
			}

			public override NNTP.NewsArticle[] GetArticleList(int startNumber, int endNumber,
				NNTP.NewsArticle.Content content)
			{
				if (this.currentGroup == null)
					throw new Exception(Errors.NoSelectedGroup);

				try
				{
					article_list articleList = webService.ArticleList(currentGroup, startNumber,
						endNumber, username, password);

					NewsArticle[] articleArray = new NewsArticle
						[articleList.articles.GetLength(0)];

					for (int i = 0; i <articleList.articles.GetLength(0); i++)
						articleArray[i] =
							ToNNTPArticle(articleList.articles[i], currentGroup, content);

					return articleArray;
				}
				catch (System.Web.Services.Protocols.SoapException)
				{
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}	
			}

			/// <summary>
			/// start article number for current griup
			/// </summary>
			protected int startNumber;
			/// <summary>
			/// end article number for current griup
			/// </summary>
			protected int endNumber;

			public override void PostMessage(string text)
			{
				try
				{
					post_result result = webService.PostMIMEMessage(username, password, text);
					if (!result.ok)
						throw new Exception(Errors.PostingFailed);
				}
				catch (System.Web.Services.Protocols.SoapException)
				{
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
				}	
			}

			protected readonly string htmlMessageTemplate;
			protected System.Text.Encoding encoding;
		}
	}
}