using System;
using System.Reflection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Collections;
using System.Web;
using System.Web.Caching;

using log4net;
using Rsdn.Framework.Common;
using Rsdn.Framework.Formatting;

using Rsdn.Mime;
using Rsdn.Nntp;
using Rsdn.Nntp.Cache;
using Rsdn.RsdnNntp.RsdnService;

namespace Rsdn.RsdnNntp
{
  /// <summary>
  /// RSDN Data Provider
  /// </summary>
  public class RsdnDataProvider : CacheDataProvider
  {
		/// <summary>
		/// Authentificate cache
		/// </summary>
		protected static Cache authCache = HttpRuntime.Cache;

    /// <summary>
    /// Cache of refeneces of messages
    /// </summary>
		protected static ReferenceCache referenceCache = new ReferenceCache(); 

		/// <summary>
    /// Filename of references cache
    /// </summary>
    protected static string referencesCacheFilename = 
    	Assembly.GetExecutingAssembly().GetName().Name + ".references.cache";

		/// <summary>
		/// Logger 
		/// </summary>
		private static ILog logger =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Global initialization
    /// </summary>
    static RsdnDataProvider()
    {
    	// load references cache
    	try
    	{
    		if (File.Exists(referencesCacheFilename))
    			referenceCache = (ReferenceCache)Deserialize(referencesCacheFilename);
    	}
    	catch (Exception e)
    	{
				logger.Error("References cache corrupted", e);
    	}

			// load message template
			using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().
							 GetManifestResourceStream("Rsdn.RsdnNntp.Header.htm"), Encoding.GetEncoding("windows-1251"), true))
			{
				htmlMessageTemplate = reader.ReadToEnd();
			}
    }

    /// <summary>
    /// Save caches at the end of work
    /// </summary>
    public override void Dispose()
    {
    	// save references cache
    	lock (referenceCache)
    		Serialize(referenceCache, referencesCacheFilename);
    }

    /// <summary>
    /// Deserialize object by binary formatter from file
    /// </summary>
    /// <param name="filename">name of file to read</param>
    /// <returns>deserialized object</returns>
    protected static object Deserialize(string filename)
    {
    	BinaryFormatter formatter = new BinaryFormatter();
    	formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
    	using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
    	{
    		return formatter.Deserialize(stream);
    	}
    }

    /// <summary>
    /// Serialize object by binary formatter to file
    /// </summary>
    /// <param name="obj">object to save</param>
    /// <param name="filename">name of file to save</param>
    protected static void Serialize(object obj, string filename)
    {
    	BinaryFormatter formatter = new BinaryFormatter();
    	formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
    	using (Stream stream = new FileStream(filename, FileMode.Create,
    						FileAccess.Write, FileShare.None))
    	{
    		formatter.Serialize(stream, obj);
    	}		
    }

		/// <summary>
		/// Construct RSDN Data Provider
		/// </summary>
    public RsdnDataProvider()
    {
    	webService = new Service();
    	encoding = System.Text.Encoding.UTF8;
    }

    /// <summary>
    /// RSDN forums' web-service proxy
    /// </summary>
    protected Service webService;
    protected string username = "";
    protected string password = "";

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
    	currentGroupArticleStartNumber = requestedGroup.first;
    	return new NewsGroup(groupName,	requestedGroup.first, requestedGroup.last,
    		requestedGroup.last - requestedGroup.first + 1, true, requestedGroup.created);
    }

    /// <summary>
    /// Update references cache
    /// </summary>
    /// <param name="message"></param>
    protected static void UpdateReferences(article message)
    {
    	lock (referenceCache)
    	{
    		referenceCache.AddReference(int.Parse(message.id), 
    			(message.pid != "") ? int.Parse(message.pid) : 0);
    	}
    }

		/// <summary>
		/// Get article by article number in specified group
		/// </summary>
		/// <param name="articleNumber"></param>
		/// <param name="groupName"></param>
		/// <param name="content"></param>
		/// <returns></returns>
    public override NewsArticle GetNonCachedArticle(int articleNumber, string groupName,
			NewsArticle.Content content)
    {
    	article message = null;
    	try
    	{
    		message = webService.GetArticle(groupName, articleNumber, username,	password);
    		if (message.error != null)
    			ProcessErrorMessage(message.error);
    	}
    	catch (System.Exception exception)
    	{
    		ProcessException(exception);
    	}	

    	// update refenece cache
    	UpdateReferences(message);

    	NewsArticle newsMessage = ToNNTPArticle(message, groupName, content);

    	return newsMessage;
    }

    static protected readonly Regex messageIdNumber =
    	new Regex(@"<(?<messageIdNumber>\d+)@news.rsdn.ru>", RegexOptions.Compiled);
    public override NewsArticle GetNonCachedArticle(string messageID, NewsArticle.Content content)
    {
    	article message = null;
		try
		{
			int mID = int.Parse(messageIdNumber.Match(messageID).Groups["messageIdNumber"].Value);
			message = webService.GetArticleByID(mID , username,	password);
			if (message.error != null)
				ProcessErrorMessage(message.error);
		}
		catch (FormatException)
		{
			throw new DataProviderException(DataProviderErrors.NoSuchArticle);
		}
		catch (System.Exception exception)
		{
			ProcessException(exception);
		}	

    	NewsArticle newsMessage = ToNNTPArticle(message, message.group, content);

    	return newsMessage;
    }

    public override NewsArticle GetNextArticle(int messageNumber, string groupName)
    {
    	NewsArticle[] articleList = GetArticleList(messageNumber + 1, int.MaxValue,
    		groupName, NewsArticle.Content.Header);
  
    	if (articleList.Length == 0)
    		throw new DataProviderException(DataProviderErrors.NoNextArticle);

    	return articleList[0];
    }

    public override NewsArticle GetPrevArticle(int messageNumber, string groupName)
    {
    	NewsArticle[] articleList = GetArticleList(currentGroupArticleStartNumber,
    		messageNumber - 1,	groupName, NewsArticle.Content.Header);
  
    	if (articleList.Length == 0)
    		throw new DataProviderException(DataProviderErrors.NoPrevArticle);

    	return articleList[articleList.Length - 1];
    }

		/// <summary>
		/// Get news groups's descriptions.
		/// </summary>
		/// <param name="startDate">Start date.</param>
		/// <param name="pattern">Match patter for groups' names. Null if none.</param>
		/// <returns></returns>
    public override NewsGroup[] GetGroupList(DateTime startDate, string pattern)
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

			Regex checker = null;
			if (pattern != null)
				checker = new Regex(pattern);

			ArrayList listOfGroups = new ArrayList(groupList.groups.Length);
    	foreach (group currentGroup in groupList.groups)
				if ((checker == null) || (checker.IsMatch(currentGroup.name)))
					listOfGroups.Add(new NewsGroup(currentGroup.name, currentGroup.first, currentGroup.last,
						currentGroup.last - currentGroup.first + 1, true, currentGroup.created));

    	return (NewsGroup[])listOfGroups.ToArray(typeof(NewsGroup));
    }

		/// <summary>
		/// Get article list.
		/// </summary>
		/// <param name="date">Start date.</param>
		/// <param name="pattern">Group name patterns.</param>
		/// <returns>List of articles.</returns>
    public override NewsArticle[] GetArticleList(System.DateTime date, string[] patterns)
    {
    	throw new DataProviderException(DataProviderErrors.NotSupported);
    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="ip"></param>
		/// <returns></returns>
		public virtual auth_info RsdnAuthentificate(string user, string pass, IPAddress ip)
		{
			return webService.Authentication(user, pass);
		}

		/// <summary>
		/// Authentificate user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="ip"></param>
		/// <returns></returns>
    public override bool Authentificate(string user, string pass, IPAddress ip)
    {
			try
			{
				// if in cache - auth ok
				UserInfo userInfo = authCache[user+pass] as UserInfo;
				if (userInfo != null)
				{
					SetUserInfo(userInfo);
					return true;
				}

    		auth_info auth = RsdnAuthentificate(user, pass, ip);
    		if (auth.ok)
    		{
					userInfo = webService.GetUserInfo(user, pass);
					// Becasuse of server stores password's hash but not clear password
					// set it to plaint text on client side 
					userInfo.Password = pass;
					// Put user information to cache for 1 hour.
					authCache.Add(user+pass, userInfo, null, Cache.NoAbsoluteExpiration,
						new TimeSpan(1, 0, 0), CacheItemPriority.AboveNormal, null);
					SetUserInfo(userInfo);
    		}
    		else
    		{
    			username = "";
    			password = "";
    		}
				return auth.ok;
			}
    	catch (System.Exception exception)
    	{
    		ProcessException(exception);
				return false;
    	}
    }

		/// <summary>
		/// Set data provider's parameters from UserInfo object.
		/// </summary>
		/// <param name="userInfo"></param>
		protected void SetUserInfo(UserInfo userInfo)
		{
			username = userInfo.Name;
			password = userInfo.Password;
			if( (rsdnSettings != null) && (rsdnSettings.Formatting == FormattingStyle.UserSettings))
				switch (userInfo.MessageFormat)
				{
					case MessageFormat.Text :
						style = FormattingStyle.PlainText;
						break;
					case MessageFormat.Html :
					case MessageFormat.TextHtml :
						style = FormattingStyle.Html;
						break;
				}
		}

    /// <summary>
    /// RSDN tags processor
    /// </summary>
    protected static readonly TextFormatter formatMessage = new TextFormatter();
    /// <summary>
    /// Result MIME messages' format.
    /// Default Html.
    /// </summary>
    protected FormattingStyle style = FormattingStyle.Html;
    /// <summary>
    /// Regular expression for detecting images in [url] tag
    /// </summary>
    protected static readonly Regex detectImages = new Regex(@"\[img\](?<url>.*?)\[/img\]",
			RegexOptions.Compiled);
    
		/// <summary>
		/// Deep of the references chain
		/// </summary>
		protected const int referencesDeep = 15;

    /// <summary>
    /// Convert rsdn's message to MIME message
    /// Also see rfc 2046, 2112, 2183, 2392, 2557
    /// </summary>
    /// <param name="message"></param>
    /// <param name="newsgroup"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    protected NewsArticle ToNNTPArticle(article message, string newsgroup,
			NewsArticle.Content content)
    {
    	NewsArticle newsMessage = new NewsArticle("<" + message.id + message.postfix + ">",
    		new string[]{newsgroup}, new int[]{message.num}, content);
    	newsMessage.HeaderEncoding = encoding;


    	if ((content == NewsArticle.Content.Header) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		newsMessage["Path"] = Session.FullHostname + "!not-for-mail";
    		if (message.author != "")
    			newsMessage.From = string.Format("\"{0}\" <{1}@news.rsdn.ru>", message.author, message.authorid);
    		newsMessage.Date = message.date;
    		newsMessage.Subject = message.subject;
    		if ((message.authorid != null) && (int.Parse(message.authorid) != 0))
    			newsMessage["X-UserID"] = message.authorid;
				newsMessage["X-MessageID"] = message.id;
    		
    		// build refences
    		StringBuilder referencesString = new StringBuilder();
    		int[] references;
    		lock (referenceCache)
    		{
    			references = referenceCache.GetReferences(int.Parse(message.id));
    		}
    		// get message's parents with limitation of depth
    		for (int i = Math.Min(references.Length - 1, referencesDeep); i > 0; i--)
    			referencesString.AppendFormat("<{0}{1}> ", references[i], message.postfix);
    		if (referencesString.Length > 0)
    			newsMessage["References"] = referencesString.ToString();
    	}

    	if ((content == NewsArticle.Content.Body) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		switch (style)
    		{
    			case FormattingStyle.PlainText :
						StringBuilder plainMessage = new StringBuilder(PrepareText(message.message));
						// for plain-text messages add some additional useful links
//						plainMessage.Append(Util.CRLF).Append(Util.CRLF).
//							Append("[purl]").Append(Util.CRLF).
//							AppendFormat("URL сообщения на сайте http://rsdn.ru/forum/?mid={0}", message.id).Append(Util.CRLF).
//							Append("[/purl]").Append(Util.CRLF);
    				newsMessage.Entities.Add(plainMessage.ToString());
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
						string userType;
						switch (message.userType)
						{
							case "rsdn": 
								userType = string.Format("<span style=\"color: red;\">{0}</span>", message.userType);
								break;
							case "эксперт": 
								userType = string.Format("<span style=\"color: green;\">{0}</span>", message.userType);
								break;
							default :
								userType = message.userType;
								break;
						}
    				string htmlText = string.Format(htmlMessageTemplate, message.authorid, message.author,
    					message.gid, message.id, formatMessage.Format(message.message, true), userType,
							formatMessage.Format(message.homePage, true), encoding.WebName,
							Format.ReplaceTags(message.subject));
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
							// set proxy setting the same as for web service
							req.Proxy = webService.Proxy;
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
    						catch (Exception ex)
						{
							logger.Error(string.Format("Image {0} not found.",
								match.Groups["url"].Value), ex);
						}
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

    public override NewsArticle[] GetArticleList(int startNumber, int endNumber, string groupName, NewsArticle.Content content)
    {
    	article_list articleList = null;
    	try
    	{
    		articleList = webService.ArticleList(groupName, startNumber, endNumber, username, password);
				if (articleList.error != null)
					ProcessErrorMessage(articleList.error);
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
    		{
    			UpdateReferences(articleList.articles[i]);
    			articleArray[i] =
    				ToNNTPArticle(articleList.articles[i], groupName, content);
    		}
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
    /// Post MIME message through data provider
    /// </summary>
    /// <param name="message"></param>
    public override void PostMessage(Message message)
    {
			try
			{
				StringBuilder postingText = GetPlainTextFromMessage(message);
				if (postingText.Length == 0)
					throw new DataProviderException(DataProviderErrors.PostingFailed, "Empty message.");

				// get message ID
				int mid = 0;
				if (message["References"] != null)
					foreach (Match messageIDMatch in messageIdNumber.Matches(message["References"]))
						mid = int.Parse(messageIDMatch.Groups["messageIdNumber"].Value);
				// get posting news group
				string group = message["Newsgroups"].Split(new char[]{','}, 2)[0].Trim();
    		
				// add tagline
				postingText.Append(Util.CRLF).Append("[tagline]Posted via " + Manager.ServerID + "[/tagline]");
    		
				post_result result = 
					webService.PostMessage(username, password, mid, group,
						Utils.ProcessInvalidXmlCharacters(message.Subject),
						Utils.ProcessInvalidXmlCharacters(postingText.ToString()));

				if (!result.ok)
					throw new DataProviderException(DataProviderErrors.PostingFailed, result.error);
			}
			catch (DataProviderException)
			{
				throw;
			}
			catch (System.Exception exception)
			{
				ProcessException(new DataProviderException(DataProviderErrors.PostingFailed, exception));
			}	
    }

		/// <summary>
		/// Template to transform formatted text to html message
		/// </summary>
    protected static readonly string htmlMessageTemplate;
		/// <summary>
		/// Message encoding
		/// </summary>
    protected System.Text.Encoding encoding;
    
    /// <summary>
    /// Process exception raised during request to data provider
    /// </summary>
    /// <param name="exception"></param>
    protected void ProcessException(System.Exception exception)
    {
    	if (typeof(InvalidOperationException).IsAssignableFrom(exception.GetType()))
    		// check for System.InvalidOperationException (html instead xml in answer),
				// System.Net.WebException (connection problems)
    		throw new DataProviderException(DataProviderErrors.ServiceUnaviable, exception);

			switch (exception.Message)
			{
				case "1 Incorrect group name." :
					throw new DataProviderException(DataProviderErrors.NoSuchGroup);
				case "2 Incorrect login name or password" :
				{
					if (authCache[username+password] != null)
						authCache.Remove(username+password);
					throw new DataProviderException(DataProviderErrors.NoPermission);
				}
				case "3 Article not found." :
					throw new DataProviderException(DataProviderErrors.NoSuchArticle);
				default:
					throw exception;
			}
		}

    /// <summary>
    /// Parse error messages from web-service
    /// </summary>
		/// <param name="message">Erorr message</param>
    protected void ProcessErrorMessage(string message)
    {
			ProcessException(new DataProviderException(message));
    }

    /// <summary>
    /// Initial session's state for this data provider
    /// </summary>
    public override Session.States InitialSessionState
    {
    	get
    	{
    		return Session.States.AuthRequired;
    	}
    }

    /// <summary>
    /// Posting are allowed or not for this data provider 
    /// </summary>
    public override bool PostingAllowed
    {
    	get
    	{
    		return true;
    	}
    }

		/// <summary>
		/// RSDN Data Provider's settings
		/// </summary>
		protected DataProviderSettings rsdnSettings;

    /// <summary>
    /// Configures data provider
    /// </summary>
    /// <param name="settings"></param>
    public override void Config(object settings)
    {
			base.Config(settings);

    	rsdnSettings = settings as DataProviderSettings;
    	if (rsdnSettings != null)
    	{
			// To fix authorization proxy bug use special middle chain
			webService = new Service();

    		webService.Url = rsdnSettings.Service;
				// set proxy if necessary
				switch (rsdnSettings.ProxyType)
				{
					case ProxyType.Default : 
						webService.Proxy = WebProxy.GetDefaultProxy();
						webService.Proxy.Credentials = CredentialCache.DefaultCredentials;
						break;
					case ProxyType.Explicit :
						webService.Proxy = rsdnSettings.Proxy;
						break;
					default:
						break;
				}
    		encoding = rsdnSettings.GetEncoding;
    		if (rsdnSettings.Formatting != FormattingStyle.UserSettings)
					style = rsdnSettings.Formatting;
    	}
    }

		/// <summary>
		/// Detect platform specific line breaks
		/// </summary>
		protected static Regex platformDependedBreak = new Regex(@"(?<!\r)\n");

	  /// <summary>
    /// remove unnecessary tags (tagline, moderator)
    /// </summary>
    protected string PrepareText(string text)
    {
    	if (text == null)
    		return "";
    	else
  		return platformDependedBreak.Replace(
				TextFormatter.RemoveTaglineTag(text), Util.CRLF);
    }

		/// <summary>
		/// Get necessary configuration object type
		/// </summary>
		/// <returns></returns>
    public override System.Type GetConfigType()
    {
    	return typeof(DataProviderSettings);
    }
	  
	/// <summary>
    /// Get only plain text from MIME message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal StringBuilder GetPlainTextFromMessage(Message message)
    {
    	StringBuilder text = new StringBuilder();
    	if ((message.ContentTypeType == "text") && (message.ContentTypeSubtype == "plain") ||
    			(message.ContentTypeType == "multipart"))
    		foreach (object entity in message.Entities)
    			if (entity is Message)
    				text.Append(GetPlainTextFromMessage((Message)entity));
    			else
    				text.Append(entity);
    	return text;
    }

		/// <summary>
		/// Identity of assembly for information purposes
		/// </summary>
    protected static readonly string identity = Assembly.GetExecutingAssembly().GetName().Name + " " +
    	Assembly.GetExecutingAssembly().GetName().Version;

		/// <summary>
		/// Identity of assembly (name version)
		/// </summary>
    public override string Identity
    {
    	get
    	{
    		return identity;
    	}
    }
  
		/// <summary>
		/// Check if provider wants specified article.
		/// </summary>
		/// <param name="messageID"></param>
		/// <returns></returns>
		public override bool WantArticle(string messageID)
		{
			return false;
		}
	
		/// <summary>
		/// Get cached article considering messag format
		/// </summary>
		/// <param name="articleNumber"></param>
		/// <param name="groupName"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public override NewsArticle GetArticle(int articleNumber, string groupName, NewsArticle.Content content)
		{
			return GetArticle(articleNumber, groupName, content, style.ToString());
		}

		/// <summary>
		/// Get cached article considering messag format
		/// </summary>
		/// <param name="originalMessageID"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public override NewsArticle GetArticle(string originalMessageID, Rsdn.Nntp.NewsArticle.Content content)
		{
			return base.GetArticle (originalMessageID, content, style.ToString());
		}
	}
}