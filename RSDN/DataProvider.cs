using System;
using derIgel.RsdnNntp.ru.rsdn;
using System.Reflection;
using System.IO;
using derIgel.NNTP;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;
using derIgel.MIME;
using RSDN.Common;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// 
	/// </summary>
	public class RsdnDataProvider : derIgel.NNTP.IDataProvider
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

		public RsdnDataProvider()
		{
			webService = new Forum();
			encoding = System.Text.Encoding.UTF8;
			Stream io = Assembly.GetExecutingAssembly().GetManifestResourceStream("derIgel.RsdnNntp.Header.htm");
			StreamReader reader = new StreamReader(io);
			htmlMessageTemplate = reader.ReadToEnd();
			reader.Close();
		}

		protected Forum webService;
		protected string username = "";
		protected string password = "";
		/// <summary>
		/// Current selected group
		/// </summary>
		protected string currentGroup = null;
		/// <summary>
		/// Current selected article
		/// </summary>
		protected int currentArticle = -1;

		public NewsArticle GetArticle(NewsArticle.Content content)
		{
			if (currentArticle == -1)
				throw new DataProviderException((currentGroup == null) ?
					DataProviderErrors.NoSelectedGroup : DataProviderErrors.NoSelectedArticle);

			return GetArticle(currentArticle, content);
		}

		public NewsGroup GetGroup(string groupName)
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

		public NewsArticle GetArticle(int articleNumber, NewsArticle.Content content)
		{
			if (currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			NewsArticle newsMessage = null;
			// access to cache
			if (cache.Capacity > 0)
				newsMessage = cache[currentGroup, articleNumber];

			if (newsMessage == null)
			{
				article message = null;
				try
				{
					message = webService.GetArticle(currentGroup, articleNumber,
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
		public NewsArticle GetArticle(string messageID, NewsArticle.Content content)
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
					message = webService.GetArticleByID(mID , username,	password);
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

		public NewsArticle GetNextArticle()
		{
			if (currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			if (currentArticle == -1)
				throw new DataProviderException(DataProviderErrors.NoSelectedArticle);

			NewsArticle[] articleList = GetArticleList(currentArticle + 1, currentGroupArticleEndNumber,
				NewsArticle.Content.Header);
	
			if (articleList.Length == 0)
				throw new DataProviderException(DataProviderErrors.NoNextArticle);

			currentArticle = articleList[0].Number;

			return articleList[0];
		}

		public NewsArticle GetPrevArticle()
		{
			if (currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			if (currentArticle == -1)
				throw new DataProviderException(DataProviderErrors.NoSelectedArticle);

			NewsArticle[] articleList = GetArticleList(currentGroupArticleStartNumber,
				currentArticle - 1,	NewsArticle.Content.Header);
	
			if (articleList.Length == 0)
				throw new DataProviderException(DataProviderErrors.NoPrevArticle);

			currentArticle = articleList[articleList.Length - 1].Number;
			return articleList[articleList.Length - 1];
		}

		public NewsGroup[] GetGroupList(DateTime startDate, string[] distributions)
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
			NewsGroup[] listOfGroups = new NewsGroup[groupList.groups.Length];
			for (int i = 0; i < groupList.groups.Length; i++)
				listOfGroups[i] = new NewsGroup(groupList.groups[i].name, groupList.groups[i].first,
					groupList.groups[i].last, groupList.groups[i].last - groupList.groups[i].first + 1,
					true);
			return listOfGroups;
		}

		public NewsArticle[] GetArticleList(string[] newsgroups, System.DateTime date,
			string[] distributions)
		{
			throw new DataProviderException(DataProviderErrors.NotSupported);
		}

		public bool Authentificate(string user, string pass)
		{
			auth_info auth = null;
			try
			{
				auth = webService.Authentication(user, pass);
				if (!auth.ok)
					throw new DataProviderException(DataProviderErrors.NoPermission);
				username = user;
				password = pass;
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
			}
			return auth.ok;
		}

		protected static readonly FormatMessage formatMessage = new FormatMessage();

		protected NewsArticle ToNNTPArticle(article message, string newsgroup, NewsArticle.Content content)
		{
			NewsArticle newsMessage = new NewsArticle("<" + message.id + message.postfix + ">",
				message.num);
			newsMessage.HeaderEncoding = encoding;

			if ((content == NewsArticle.Content.Header) ||
				(content == NewsArticle.Content.HeaderAndBody))
			{
				if (message.author != "")
					newsMessage.From = string.Format("\"{0}\" <{1}>", message.author, "forum@rsdn.ru");
				newsMessage.Date = message.date;
				newsMessage.Subject = message.subject;
				newsMessage["X-UserID"] = message.authorid;
				if (message.pid != string.Empty)
					newsMessage["References"] = "<" + message.pid + message.postfix + ">";
				newsMessage["Newsgroups"] = newsgroup;
				newsMessage["X-Server"] = serverID;
			}

			if ((content == NewsArticle.Content.Body) ||
				(content == NewsArticle.Content.HeaderAndBody))
			{
				Message plainTextBody = new Message();
				plainTextBody.Entities.Add(PrepareText(message.message));
				plainTextBody.TransferEncoding = ContentTransferEncoding.Base64;
				plainTextBody.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.BodyName);

				string htmlText = string.Format(htmlMessageTemplate, message.authorid, message.author,
					message.gid, message.id,
					(message.message != null) ? formatMessage.PrepareText(message.message, true) : null,
					message.userType,
					(message.homePage != null) ? formatMessage.PrepareText(message.homePage, true) : null);
				Message htmlTextBody = new Message();
				htmlTextBody.Entities.Add(htmlText);
				htmlTextBody.TransferEncoding = ContentTransferEncoding.Base64;
				htmlTextBody.ContentType = string.Format("text/html; charset=\"{0}\"", encoding.BodyName);

				newsMessage.Entities.Add(plainTextBody);
				newsMessage.Entities.Add(htmlTextBody);
				newsMessage.ContentType = "multipart/alternative";
			}
	
			return newsMessage;
		}

		public NNTP.NewsArticle[] GetArticleList(int startNumber, int endNumber,
			NNTP.NewsArticle.Content content)
		{
			if (this.currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			article_list articleList = null;
			try
			{
				articleList = webService.ArticleList(currentGroup,
					(startNumber == -1) ? currentGroupArticleStartNumber : startNumber,
					(endNumber == -1) ? currentGroupArticleEndNumber : endNumber, username, password);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
			}	

			NewsArticle[] articleArray = new NewsArticle[articleList.articles.Length];

			for (int i = 0; i <articleList.articles.Length; i++)
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

		protected static readonly string serverID = Manager.GetProductTitle(Assembly.GetExecutingAssembly());

		protected static Regex leadingSpaces = new Regex(@"(?m)^\s+", RegexOptions.Compiled);

		public void PostMessage(Message message)
		{
			try
			{
				int mid = 0;
				if (message["References"] != null)
					foreach (Match messageIDMatch in messageIdNumber.Matches(message["References"]))
						mid = int.Parse(messageIDMatch.Groups["messageIdNumber"].Value);
				string group = message["Newsgroups"].Split(new char[]{','}, 2)[0].Trim();
				StringBuilder plainText = new StringBuilder();
				foreach (string text in message.Entities)
					plainText.Append(text);
				
				// process wrong encoding
				if (plainText.ToString().IndexOf("????") > 0)
					throw new DataProviderException(DataProviderErrors.PostingFailed);

				// tagline
				plainText.Append("[tagline]Posted via ").Append(serverID).Append("[/tagline]");

				post_result result =
					webService.PostUnicodeMessage(username, password, mid, group, message.Subject,
					leadingSpaces.Replace(plainText.ToString(), ""));
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
			if (exception.GetType() == typeof(System.Net.WebException))
				// problems with connection?
				throw new DataProviderException(DataProviderErrors.ServiceUnaviable, exception);

			// if not handeled - throw forward
			throw exception;
		}

		protected void ProcessErrorMessage(string message)
		{
			switch (message)
			{
				case "1 Incorrect group name." :
					throw new DataProviderException(DataProviderErrors.NoSuchGroup);
				case "2 Incorrect login name or password" :
					throw new DataProviderException(DataProviderErrors.NoPermission);
				case "3 Article not found." :
					throw new DataProviderException(DataProviderErrors.NoSuchArticle);
				case "Timeout expired." +
					"  The timeout period elapsed prior to completion of the operation or the server is not responding." :
					throw new DataProviderException(DataProviderErrors.ServiceUnaviable);
				default:
					throw new DataProviderException(DataProviderErrors.UnknownError);
			}
		}

		public derIgel.NNTP.Session.States InitialSessionState
		{
			get
			{
				return Session.States.AuthRequired;
			}
		}

		public bool PostingAllowed
		{
			get
			{
				return true;
			}
		}

		public void Config(derIgel.NNTP.NNTPSettings settings)
		{
			DataProviderSettings rsdnSettings = settings as DataProviderSettings;
			if (rsdnSettings != null)
			{
				webService.Url = rsdnSettings.Service;
				webService.Proxy = rsdnSettings.Proxy;
				encoding = rsdnSettings.GetEncoding;
				cache.Capacity = rsdnSettings.CacheSize;
			}
		}

		protected static Regex removeTagline = new Regex(@"(?s)\[tagline\].*?\[/tagline\]", RegexOptions.Compiled);
		protected static Regex moderatorTagline = new Regex(@"(?s)\[moderator\].*?\[/moderator\]",
			RegexOptions.Compiled);

		/// <summary>
		/// remove unnecessary tags (tagline, moderator)
		/// </summary>
		protected string PrepareText(string text)
		{
			if (text == null)
				return null;
			else
				return moderatorTagline.Replace(removeTagline.Replace(text, ""), "");
		}
	}
}