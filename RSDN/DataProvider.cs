using System;
using derIgel.RsdnNntp.ru.rsdn;
using System.Reflection;
using System.IO;
using derIgel.NNTP;
using System.Net;
using derIgel.Mail;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;

namespace derIgel.RsdnNntp
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
			initialSessionState = Session.States.AuthRequired;
			postingAllowed = true;
			webService = new Forum();
			encoding = System.Text.Encoding.UTF8;
			RsdnDataProviderSettings rsdnSettings = settings as RsdnDataProviderSettings;
			if (rsdnSettings != null)
			{
				webService.Url = ((RsdnDataProviderSettings)rsdnSettings).Service;
				webService.Proxy = rsdnSettings.GetProxy;
				encoding = rsdnSettings.GetEncoding;
				cache.Capacity = rsdnSettings.CacheSize;
			}
			Stream io = Assembly.GetExecutingAssembly().GetManifestResourceStream("derIgel.RsdnNntp.Header.htm");
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
				if (requestedGroup.error != null)
					ProcessErrorMessage(requestedGroup.error);
			}		
			catch (System.Exception exception)
			{
				ProcessException(exception);
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
			// access to cache
			if (cache.Capacity > 0)
				newsMessage = cache[currentGroup, articleNumber];

			if (newsMessage == null)
			{
				article message = null;
				try
				{
					message = webService.GetFormattedArticle(currentGroup, articleNumber,
						username,	password);
					if (message.error != null)
						ProcessErrorMessage(message.error);
				}
				catch (System.Exception exception)
				{
					ProcessException(exception);
				}	

				currentArticle = articleNumber;
				newsMessage = ToNNTPArticle(message, currentGroup, content);
				// access to cache
				if (cache.Capacity > 0)
					cache[newsMessage.MessageID, currentGroup, articleNumber] =	newsMessage;
			}

			return newsMessage;
		}

		static protected readonly Regex messageIdNumber =
			new Regex(@"<(?<messageIdNumber>\d+)@\S+>", RegexOptions.Compiled);
		public override NewsArticle GetArticle(string messageID, NewsArticle.Content content)
		{
			NewsArticle newsMessage = null;
			// access to cache
			if (cache.Capacity > 0)
				newsMessage = cache[messageID];

			if (newsMessage == null)
			{
				article message = null;
				try
				{
					int mID = int.Parse(messageIdNumber.Match(messageID).Groups["messageIdNumber"].Value);
					message = webService.GetFormattedArticleByID(mID , username,	password);
					if (message.error != null)
						ProcessErrorMessage(message.error);
				}
				catch (System.Exception exception)
				{
					ProcessException(exception);
				}	

				newsMessage = ToNNTPArticle(message, message.group, content);
				// access to cache
				if (cache.Capacity > 0)
					cache[newsMessage.MessageID, currentGroup, newsMessage.Number] =	newsMessage;
			}

			return newsMessage;
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
				if (groupList.error != null)
					ProcessErrorMessage(groupList.error);
			}				
			catch (System.Exception exception)
			{
				ProcessException(exception);
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
				if (!auth.ok)
					throw new Exception(Errors.PostingFailed);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
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
				string htmlText = string.Format(htmlMessageTemplate, message.authorid, message.author,
					message.gid, message.id, message.fmtmessage, message.userType, message.homePage);
				Message htmlTextBody = new Message();
				htmlTextBody.Bodies.Add(/*encoding.GetString(outputStream.GetBuffer())*/htmlText);
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
				if (articleList.error != null)
					ProcessErrorMessage(articleList.error);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
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

		public override void PostMessage(byte[] message)
		{
			try
			{
				post_result result = webService.PostMIMEMessage(username, password,
					Encoding.GetEncoding("iso-8859-1").GetString(message));
				if (!result.ok)
					ProcessErrorMessage(result.error);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
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

		protected void ProcessException(System.Exception exception)
		{
			if (exception.GetType() == typeof(System.Web.Services.Protocols.SoapException))
			{
				ProcessErrorMessage(Regex.Match(exception.Message, @"^.+ ---> .+: (?<error>.+)").Groups["error"].Value);
			}
			else
				if (exception.GetType() == typeof(System.Net.WebException))
				throw new Exception(Errors.ServiceUnaviable, exception);

			// if not handeled - throw forward
			throw exception;
		}

		protected void ProcessErrorMessage(string message)
		{
			switch (message)
			{
				case "1 Incorrect group name" :
					throw new Exception(Errors.NoSuchGroup);
				case "2 Incorrect login name or password" :
					throw new Exception(Errors.NoPermission);
				case "3 Article not found." :
					throw new Exception(Errors.NoSuchArticle);
				case "Timeout expired." +
					"  The timeout period elapsed prior to completion of the operation or the server is not responding." :
					throw new Exception(Errors.ServiceUnaviable);
				default:
					throw new DataProvider.Exception(DataProvider.Errors.UnknownError);
			}
		}
	}
}