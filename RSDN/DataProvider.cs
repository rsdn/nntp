// $Id$
using System;
using System.Reflection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;
using System.Web;

using RSDN.Common;

using Rsdn.Mime;
using Rsdn.Nntp;
using Rsdn.RsdnNntp.ru.rsdn;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// RSDN Data Provider
	/// </summary>
	public class RsdnDataProvider : IDataProvider
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
			Stream io = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rsdn.RsdnNntp.Header.htm");
			StreamReader reader = new StreamReader(io);
			htmlMessageTemplate = reader.ReadToEnd();
			reader.Close();
		}

		/// <summary>
		/// RSDN forums' web-service proxy
		/// </summary>
		protected Forum webService;
		protected string username = "";
		protected string password = "";
		/// <summary>
		/// Currently selected group
		/// </summary>
		protected string currentGroup = null;
		/// <summary>
		/// Currently selected article
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
					cache[newsMessage.MessageID, currentGroup, (int)newsMessage.MessageNumbers[currentGroup]] =	newsMessage;
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

			currentArticle = (int)articleList[0].MessageNumbers[currentGroup];

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

			currentArticle = (int)articleList[articleList.Length - 1].MessageNumbers[currentGroup];
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
				if (auth.ok)
				{
					username = user;
					password = pass;
				}
				else
				{
					username = "";
					password = "";
				}
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
			}
			return auth.ok;
		}

		/// <summary>
		/// RSDN tags processor
		/// </summary>
		protected static readonly FormatMessage formatMessage = new FormatMessage();
		/// <summary>
		/// Result MIME messages' format
		/// </summary>
		protected FormattingStyle style = FormattingStyle.Html;
		/// <summary>
		/// Regular expression for detecting images in [url] tag
		/// </summary>
		protected static readonly Regex detectImages = new Regex(@"\[img\](?<url>.*?)\[/img\]", RegexOptions.Compiled);
		
		/// <summary>
		/// Convert rsdn's message to MIME message
		/// Also see rfc 2046, 2112, 2183, 2392, 2557
		/// </summary>
		/// <param name="message"></param>
		/// <param name="newsgroup"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		protected NewsArticle ToNNTPArticle(article message, string newsgroup, NewsArticle.Content content)
		{
			NewsArticle newsMessage = new NewsArticle("<" + message.id + message.postfix + ">",
				new string[]{newsgroup}, new int[]{message.num});
			newsMessage.HeaderEncoding = encoding;

			if ((content == NewsArticle.Content.Header) ||
				(content == NewsArticle.Content.HeaderAndBody))
			{
				newsMessage["Path"] = Session.FullHostname + "!not-for-mail";
				if (message.author != "")
					newsMessage.From = string.Format("\"{0}\" <{1}>", message.author, "forum@rsdn.ru");
				newsMessage.Date = message.date;
				newsMessage.Subject = message.subject;
				if ((message.authorid != null) && (int.Parse(message.authorid) != 0))
					newsMessage["X-UserID"] = message.authorid;
				if (message.pid != string.Empty)
					newsMessage["References"] = "<" + message.pid + message.postfix + ">";
			}

			if ((content == NewsArticle.Content.Body) ||
				(content == NewsArticle.Content.HeaderAndBody))
			{
				switch (style)
				{
					case FormattingStyle.PlainText :
						newsMessage.Entities.Add(PrepareText(message.message));
						newsMessage.TransferEncoding = ContentTransferEncoding.Base64;
						newsMessage.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.WebName);
						break;
					case FormattingStyle.Html :
					case FormattingStyle.HtmlInlineImages :
						Message plainTextBody = new Message(false);
						plainTextBody.Entities.Add(PrepareText(message.message));
						plainTextBody.TransferEncoding = ContentTransferEncoding.Base64;
						plainTextBody.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.WebName);

						Message htmlTextBody = new Message(false);
						string htmlText = string.Format(htmlMessageTemplate, message.authorid, message.author,
							message.gid, message.id,
							(message.message != null) ? formatMessage.PrepareText(message.message, true) : "",
							message.userType,
							(message.homePage != null) ? formatMessage.PrepareText(message.homePage, true) : null);
						htmlTextBody.Entities.Add(htmlText);
						htmlTextBody.TransferEncoding = ContentTransferEncoding.Base64;
						htmlTextBody.ContentType = string.Format("text/html; charset=\"{0}\"", encoding.WebName);
						
						MatchCollection detectedImages = detectImages.Matches((message.message != null) ? message.message : "");
						if ((style == FormattingStyle.HtmlInlineImages) && (detectedImages.Count > 0))
						{
							newsMessage.ContentType = "multipart/related; type=multipart/alternative";

							Message combineMessage = new Message(false);
							combineMessage.ContentType = "multipart/alternative";
							combineMessage.Entities.Add(plainTextBody);
							combineMessage.Entities.Add(htmlTextBody);
							newsMessage.Entities.Add(combineMessage);

							foreach (Match match in detectedImages)
							{
								WebResponse response = null;
								try
								{
									WebRequest req = WebRequest.Create(match.Groups["url"].Value);
									response = req.GetResponse();
									Message imgPart = new Message(false);
									imgPart.ContentType = response.ContentType;
									Guid imgContentID = Guid.NewGuid();
									imgPart["Content-ID"] = '<' + imgContentID.ToString() + '>';
									imgPart["Content-Location"] = req.RequestUri.ToString();
									imgPart["Content-Disposition"] = "inline";
									imgPart.TransferEncoding = ContentTransferEncoding.Base64;
									using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
									{
										imgPart.Entities.Add(reader.ReadBytes((int)response.ContentLength));
									}
									newsMessage.Entities.Add(imgPart);
									htmlText = htmlText.Replace(match.Groups["url"].Value, "cid:" + imgContentID.ToString());
								}
								catch (Exception) {}
								finally
								{
									if (response != null)
										response.Close();
								}
							}
							htmlTextBody.Entities[0] = htmlText;
						}
						else
						{
							newsMessage.ContentType = "multipart/alternative";
							newsMessage.Entities.Add(plainTextBody);
							newsMessage.Entities.Add(htmlTextBody);
						}
						break;
				}
			}
	
			return newsMessage;
		}

		public NewsArticle[] GetArticleList(int startNumber, int endNumber, NewsArticle.Content content)
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

			NewsArticle[] articleArray;
			// sometimes web-service return null....
			if (articleList != null)
			{
				articleArray = new NewsArticle[articleList.articles.Length];

				for (int i = 0; i < articleList.articles.Length; i++)
					articleArray[i] =
						ToNNTPArticle(articleList.articles[i], currentGroup, content);
			}
			else
				articleArray = new NewsArticle[0];

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

		protected static Regex leadingSpaces = new Regex(@"(?m)^[\t ]+", RegexOptions.Compiled);

		/// <summary>
		/// refular expression for detecting Re: & Re[number]: prefixes in subject at the start of the line
		/// </summary>
		protected static Regex reDetecter =
			new Regex(@"^((Re\[(?<num>\d+)\]|Re(?<num>.{0,0})):\s*)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		/// <summary>
		/// Post MIME message through data provider
		/// </summary>
		/// <param name="message"></param>
		public void PostMessage(Message message)
		{
			try
			{
				string postingText = GetPlainTextFromMessage(message);
				if (postingText == "")
					throw new DataProviderException(DataProviderErrors.PostingFailed);

				// get message ID
				int mid = 0;
				if (message["References"] != null)
					foreach (Match messageIDMatch in messageIdNumber.Matches(message["References"]))
						mid = int.Parse(messageIDMatch.Groups["messageIdNumber"].Value);
				// get posting news group
				string group = message["Newsgroups"].Split(new char[]{','}, 2)[0].Trim();
				
				// tagline
				postingText += Util.CRLF + "[tagline]Posted via " + Manager.ServerID + "[/tagline]";
				
				// by default use original subject
				string subject = message.Subject;

				// check & modify subject if necessary (prevent 'Re: Re[2]' problem)
				Match match = reDetecter.Match(message.Subject);
				if (match.Success)
				{
					int answersCount = 0;
					foreach(Capture capture in match.Groups["num"].Captures)
						if (capture.Value == "")
							answersCount ++;
						else
							answersCount += int.Parse(capture.Value);
					
					if (answersCount > 1)
						subject = string.Format("Re[{0}]: ", answersCount);
					else
						subject = "Re: ";
					subject += reDetecter.Replace(message.Subject, "");
				}

				post_result result = 
					webService.PostUnicodeMessage(username, password, mid, group, subject,
						leadingSpaces.Replace(postingText, ""));

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

		/// <summary>
		/// Process exception raised during request to data provider
		/// </summary>
		/// <param name="exception"></param>
		protected void ProcessException(System.Exception exception)
		{
			if (exception.GetType() == typeof(System.Net.WebException))
				// problems with connection?
				throw new DataProviderException(DataProviderErrors.ServiceUnaviable, exception);

			// if not handeled - throw forward
			throw exception;
		}

		/// <summary>
		/// Parse error messages from web-service
		/// </summary>
		/// <param name="message"></param>
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

		/// <summary>
		/// Initial session's state for this data provider
		/// </summary>
		public Session.States InitialSessionState
		{
			get
			{
				return Session.States.AuthRequired;
			}
		}

		/// <summary>
		/// Posting are allowed or not for this data provider 
		/// </summary>
		public bool PostingAllowed
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Configures data provider
		/// </summary>
		/// <param name="settings"></param>
		public void Config(object settings)
		{
			DataProviderSettings rsdnSettings = settings as DataProviderSettings;
			if (rsdnSettings != null)
			{
				webService.Url = rsdnSettings.Service;
				webService.Proxy = rsdnSettings.Proxy;
				encoding = rsdnSettings.GetEncoding;
				cache.Capacity = rsdnSettings.CacheSize;
				style = rsdnSettings.Formatting;
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

		/// <summary>
		/// currently selected news group
		/// </summary>
		public string CurrentGroup
		{
			get
			{
				return currentGroup;
			}
		}

		public System.Type GetConfigType()
		{
			return typeof(DataProviderSettings);
		}

		/// <summary>
		/// Get only plain text from MIME message
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		internal string GetPlainTextFromMessage(Message message)
		{
			StringBuilder text = new StringBuilder();
			if ((message.ContentTypeType == "text") && (message.ContentTypeSubtype == "plain") ||
				  (message.ContentTypeType == "multipart"))
				foreach (object entity in message.Entities)
					if (entity is Message)
						text.Append(GetPlainTextFromMessage((Message)entity));
					else
						text.Append(entity);
			return text.ToString();
		}

		protected static readonly string identity = Assembly.GetExecutingAssembly().GetName().Name + " " +
			Assembly.GetExecutingAssembly().GetName().Version;

		public string Identity
		{
			get
			{
				return identity;
			}
		}
	}
}