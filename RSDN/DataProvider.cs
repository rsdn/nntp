using System;
using System.Collections.Specialized;
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
				try
				{
					ProcessException(exception);
				}
				catch (DataProviderException providerEx)
				{
					if (providerEx.Error == DataProviderErrors.NoSuchGroup)
					{
						throw new DataProviderException(DataProviderErrors.NoSuchGroup, groupName, providerEx);
					}
					else
						throw;
				}
    	}
    	currentGroupArticleStartNumber = requestedGroup.first;
    	return new NewsGroup(groupName,	requestedGroup.first, requestedGroup.last,
    		requestedGroup.last - requestedGroup.first + 1, true, requestedGroup.created);
    }

    /// <summary>
    /// Update references cache
    /// </summary>
    /// <param name="message">Message, contains reference information</param>
    /// <returns>The same, original message</returns>
    protected static article UpdateReferences(article message)
    {
    	lock (referenceCache)
    	{
    		referenceCache.AddReference(int.Parse(message.id), 
    			(message.pid != "") ? int.Parse(message.pid) : 0);
				return message;
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

    	// update refenece cache & transform to another message format
    	NewsArticle newsMessage = ToNNTPArticle(UpdateReferences(message), groupName, content);

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

    	NewsArticle newsMessage = ToNNTPArticle(UpdateReferences(message), message.group, content);

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
    	group_list groupList = cache["$group_list_cache$"] as group_list;
			if (groupList == null)
    		try
    		{
    			groupList = webService.GetGroupList(username, password, DateTime.MinValue);
    			if (groupList.error != null)
    				ProcessErrorMessage(groupList.error);
					// add group list to cache with 15 minitues sliding expiration
					cache.Add("$group_list_cache$", groupList, null, Cache.NoAbsoluteExpiration,
						new TimeSpan(0, 15, 0), CacheItemPriority.AboveNormal, null);
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
				if (currentGroup.created >= startDate &&
						((checker == null) || (checker.IsMatch(currentGroup.name))))
					listOfGroups.Add(new NewsGroup(currentGroup.name, currentGroup.first, currentGroup.last,
						currentGroup.last - currentGroup.first + 1, true, currentGroup.created));

    	return (NewsGroup[])listOfGroups.ToArray(typeof(NewsGroup));
    }

		/// <summary>
		/// Get article list.
		/// </summary>
		/// <param name="date">Start date.</param>
		/// <param name="pattern">Group name pattern.</param>
		/// <returns>List of articles.</returns>
    public override NewsArticle[] GetArticleList(System.DateTime date, string pattern)
    {
			ArrayList groups = new ArrayList();

			// get all appropriated groups
			foreach (NewsGroup group in GetGroupList(DateTime.MinValue, pattern))
				groups.Add(group.Name);

			article_list articleList = null;
			try
			{
				articleList = webService.ArticleListFromDate((string[])groups.ToArray(typeof(string)), date, username, password);
				if (articleList.error != null)
					ProcessErrorMessage(articleList.error);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
			}	

			ArrayList articles =  new ArrayList();

			// sometimes web-service return null....
			if (articleList != null)
				// process messages
				foreach (article message in articleList.articles)
					articles.Add(ToNNTPArticle(UpdateReferences(message), message.group, NewsArticle.Content.Header));

			return (NewsArticle[])articles.ToArray(typeof(NewsArticle));
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
			if (Utils.InvalidXmlCharacters.IsMatch(user))
				throw new DataProviderException(DataProviderErrors.NoPermission,
					"Username contains not allowed symbols.");
			if (Utils.InvalidXmlCharacters.IsMatch(pass))
				throw new DataProviderException(DataProviderErrors.NoPermission,
					"Password contains not allowed symbols.");
				
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
				string cacheKey = user + pass;
				// if in cache - auth ok
				UserInfo userInfo = cache[cacheKey] as UserInfo;
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
					cache.Add(cacheKey, userInfo, null, Cache.NoAbsoluteExpiration,
						new TimeSpan(1, 0, 0), CacheItemPriority.AboveNormal, null);
					cache.Insert("userId$" + userInfo.ID, userInfo,
						new CacheDependency(null, new string[]{cacheKey}));

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

		protected UserInfo GetUserInfo(int id)
		{
			try
			{
				UserInfo userInfo = cache.Get("userId$" + id) as UserInfo;
				if (userInfo == null)
				{
					userInfo = webService.GetUserInfoByID(username, password, id);
					// Put user information to cache for 1 hour.
					cache.Add("userId$" + userInfo.ID, userInfo, null, Cache.NoAbsoluteExpiration,
						new TimeSpan(1, 0, 0), CacheItemPriority.Normal, null);
				}
				return userInfo;
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
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
    /// Result MIME messages' format.
    /// Default Html.
    /// </summary>
    protected FormattingStyle style = FormattingStyle.Html;
    
		/// <summary>
		/// Deep of the references chain
		/// </summary>
		protected const int referencesDeep = 7;

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

			NntpTextFormatter formatMessage =
				new NntpTextFormatter(serverName, webService.Proxy, style);

			NewsArticle newsMessage = new NewsArticle("<" + message.id + message.postfix + ">",
    		new string[]{newsgroup}, new int[]{message.num}, content);
    	newsMessage.HeaderEncoding = encoding;

    	if ((content == NewsArticle.Content.Header) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		newsMessage["Path"] = Session.FullHostname + "!not-for-mail";
    		if (message.author != "")
    			newsMessage.From = string.Format("\"{0}\" <{1}@users.rsdn.ru>", message.author, message.authorid);
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
    		// get message's parents from the start and from the end with limitation of depth
				for (int i = references.Length - 1;
					(i >= references.Length - referencesDeep) && (i > 0); i--)
					referencesString.AppendFormat("<{0}{1}> ", references[i], message.postfix);
				for (int i = Math.Min(references.Length - referencesDeep - 1, referencesDeep); i > 0; i--)
    			referencesString.AppendFormat("<{0}{1}> ", references[i], message.postfix);
    		if (referencesString.Length > 0)
    			newsMessage["References"] = referencesString.ToString();
				if (references.Length > 1)
					newsMessage["In-Reply-To"] = string.Format("<{0}{1}> ", references[1], message.postfix);
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
//							AppendFormat("URL ��������� �� ����� http://rsdn.ru/forum/?mid={0}", message.id).Append(Util.CRLF).
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
						string userType = "";
						if (message.userType != null && message.userType.Length > 0)
						{
							userType = string.Format("<span style=\"color: #{0:x6}; font-size: smaller;\">{1}</span>",
								message.userColor, message.userType);
						}

						UserInfo userInfo = GetUserInfo(Format.ToInt(message.authorid));

    				string htmlText = string.Format(htmlMessageTemplate, message.authorid, message.author,
    					message.gid, message.id, formatMessage.Format(message.message, message.smile), userType,
							formatMessage.Format(message.homePage, message.smile), encoding.WebName,
							Format.ReplaceTags(message.subject), serverSchemeAndName,
							Format.ToInt(message.authorid) != 0 ?
								string.Format("href='/Users/Profile.aspx?uid={0}'", message.authorid) : null,
							formatMessage.Format(userInfo == null ? null : userInfo.Origin , true));
    				htmlTextBody.Entities.Add(htmlText);
    				htmlTextBody.TransferEncoding = ContentTransferEncoding.Base64;
    				htmlTextBody.ContentType = string.Format("text/html; charset=\"{0}\"", encoding.WebName);
    				
    				if (formatMessage.ProcessedImagesCount > 0 )
    				{
    					newsMessage.ContentType = "multipart/related; type=multipart/alternative";

    					Message combineMessage = new Message(false);
    					combineMessage.ContentType = "multipart/alternative";
    					combineMessage.Entities.Add(plainTextBody);
    					combineMessage.Entities.Add(htmlTextBody);
    					newsMessage.Entities.Add(combineMessage);

    					foreach (Message img in formatMessage.GetProcessedImages())
   							newsMessage.Entities.Add(img);
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
    			articleArray[i] =
    				ToNNTPArticle(UpdateReferences(articleList.articles[i]), groupName, content);
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
    		
				// process attachments
				if (true)
				{
					NameValueCollection processedFiles = new NameValueCollection();
					foreach (Message entity in message.Entities)
					{
						string disposition = entity.GetHeaderFieldValue("Content-Disposition");
						if ((disposition != null) && (disposition.ToLower().Equals("attachment")))
						{
							string filename =
								entity.GetHeaderFieldParameters("Content-Disposition")["filename"];

							if (filename == null)
								filename = Guid.NewGuid().ToString();

							// post file ....
							byte[] binaryFile = new byte[0];
							foreach (object body in entity.Entities)
							{
								byte[] binaryBody;
								if (body is byte[])
								{
									binaryBody = (byte[])body;
								}
								else
									binaryBody = entity.Encoding.GetBytes(body.ToString());
								byte[] newBinaryFile = new byte[binaryFile.Length + binaryBody.Length];
								Buffer.BlockCopy(binaryFile, 0, newBinaryFile, 0, binaryFile.Length);
								Buffer.BlockCopy(binaryBody, 0, newBinaryFile, binaryFile.Length, binaryBody.Length);
								binaryFile = newBinaryFile;
							}

							FileInfo file = new FileInfo(
								Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
								filename));

							if (file.Exists)
								throw new DataProviderException(DataProviderErrors.PostingFailed,
									"Attached file(s) already exists.");

							using (FileStream stream = file.Create())
							{
								stream.Write(binaryFile, 0, binaryFile.Length);
							}

							//filename, 
							//string.Format("{0}/{1}",
							//entity.ContentTypeType, entity.ContentTypeSubtype), binaryFile
							processedFiles[filename] = "file://" + file.FullName;
						}
					}

					if (processedFiles.Count > 0)
					{
						postingText.Append(Util.CRLF).Append("Attached files:");
						foreach (string filename in processedFiles.AllKeys)
						{
							postingText.Append(Util.CRLF).
								AppendFormat("[url={1}]{0}[/url]", filename, processedFiles[filename]);
						}
					}
				}

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
					if (cache[username+password] != null)
						cache.Remove(username+password);
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
		/// Server address (scheme & host name) used to generate site links.
		/// Retrivied from web service address.
		/// Only top level hosting supported.
		/// </summary>
		protected String serverSchemeAndName = "http://" + Format.RsdnDomainName;


		/// <summary>
		/// Server address (only host) used to generate internal site links.
		/// Retrivied from web service address.
		/// Only top level hosting supported.
		/// </summary>
		protected String serverName = Format.RsdnDomainName;
		
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
				webService = rsdnSettings.EnableHttpCompression ?
					new CompressService() : new Service();

    		serverSchemeAndName = rsdnSettings.ServiceUri.GetLeftPart(UriPartial.Authority);
				serverName = rsdnSettings.ServiceUri.Host;
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