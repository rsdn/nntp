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

using log4net;
using Rsdn.Framework.Formatting;

using Rsdn.Mime;
using Rsdn.Nntp;
using Rsdn.Nntp.Cache;
using Rsdn.RsdnNntp.ru.rsdn;

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
		protected static ArrayList authCache = new ArrayList();

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
    /// Filename of messages cache
    /// </summary>
    protected static string cacheFilename =
    	Assembly.GetExecutingAssembly().GetName().Name + ".cache";

		/// <summary>
		/// Logger 
		/// </summary>
		private static ILog logger =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Read caches at the start
    /// </summary>
    static RsdnDataProvider()
    {
    	// load messages cache
//    	try
//    	{
//    		if (File.Exists(cacheFilename))
//    			cache = (Cache)Deserialize(cacheFilename);
//    	}
//    	catch (Exception e)
//    	{
//    		logger.Error("Messages cache corrupted", e);
//    	}

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
    }

    /// <summary>
    /// Save caches at the end of work
    /// </summary>
    public override void Dispose()
    {
//    	// save message cache
//    	lock (cache)
//    		Serialize(cache, cacheFilename);
    	// save references cache
    	lock (referenceCache)
    		Serialize(referenceCache, referencesCacheFilename);

			base.Dispose();
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
    		requestedGroup.last - requestedGroup.first + 1, true);
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
    public override NewsArticle GetNonCachedArticle(int articleNumber, string groupName, NewsArticle.Content content)
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
    	new Regex(@"<(?<messageIdNumber>\d+)@\S+>", RegexOptions.Compiled);
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
						currentGroup.last - currentGroup.first + 1, true));

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

    public override bool Authentificate(string user, string pass)
    {
			// if in cache - auth ok
			if (authCache.Contains(user.GetHashCode()))
			{
				username = user;
				password = pass;
				return true;
			}

    	auth_info auth = null;
    	try
    	{
    		auth = webService.Authentication(user, pass);
    		if (auth.ok)
    		{
    			username = user;
    			password = pass;
					authCache.Add(user.GetHashCode());
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
    protected static readonly TextFormatter formatMessage = new TextFormatter();
    /// <summary>
    /// Result MIME messages' format
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
    					message.gid, message.id, formatMessage.Format(message.message, true),
    					(message.userType == "rsdn") ? string.Format("<span style=\"color: red;\">{0}</span>", message.userType) : message.userType,
						formatMessage.Format(message.homePage, true));
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

    public override NewsArticle[] GetArticleList(int startNumber, int endNumber, string groupName, NewsArticle.Content content)
    {
    	article_list articleList = null;
    	try
    	{
    		articleList = webService.ArticleList(groupName, startNumber, endNumber, username, password);
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
    		
				post_result result = 
					webService.PostUnicodeMessage(username, password, mid, group, message.Subject, postingText);

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
    protected readonly string htmlMessageTemplate;
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
    	if (exception.GetType() == typeof(System.Net.WebException))
    		// problems with connection?
    		throw new DataProviderException(DataProviderErrors.ServiceUnaviable, exception);

    	// if not handeled - throw forward
    	throw exception;
    }

    /// <summary>
    /// Parse error messages from web-service
    /// </summary>
		/// <param name="message">Erorr message</param>
    protected void ProcessErrorMessage(string message)
    {
    	switch (message)
    	{
    		case "1 Incorrect group name." :
    			throw new DataProviderException(DataProviderErrors.NoSuchGroup);
				case "2 Incorrect login name or password" :
				{
					authCache.Remove(username.GetHashCode());
					throw new DataProviderException(DataProviderErrors.NoPermission);
				}
    		case "3 Article not found." :
    			throw new DataProviderException(DataProviderErrors.NoSuchArticle);
    		case "Timeout expired." +
    			"  The timeout period elapsed prior to completion of the operation or the server is not responding." :
    			throw new DataProviderException(DataProviderErrors.ServiceUnaviable);
    		default:
    			throw new DataProviderException(message);
    	}
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
    /// Configures data provider
    /// </summary>
    /// <param name="settings"></param>
    public override void Config(object settings)
    {
			base.Config(settings);

    	DataProviderSettings rsdnSettings = settings as DataProviderSettings;
    	if (rsdnSettings != null)
    	{
    		webService.Url = rsdnSettings.Service;
				// set proxy if necessary
				switch (rsdnSettings.ProxyType)
				{
					case ProxyType.Default : 
						webService.Proxy = WebProxy.GetDefaultProxy();
						break;
					case ProxyType.Explicit :
						webService.Proxy = rsdnSettings.Proxy;
						break;
					default:
						break;
				}
    		encoding = rsdnSettings.GetEncoding;
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
    		return null;
    	else
  		return platformDependedBreak.Replace(
				TextFormatter.RemoveTaglineTag(TextFormatter.RemoveModeratorTag(text)), Util.CRLF);
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
	}
}