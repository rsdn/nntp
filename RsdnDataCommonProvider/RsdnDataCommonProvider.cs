using System;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using System.Web.Caching;

using log4net;
using Rsdn.Framework.Common;
using Rsdn.Framework.Formatting;

using Rsdn.Mime;
using Rsdn.Nntp;
using Rsdn.Nntp.Cache;

namespace Rsdn.RsdnNntp.Common
{
  /// <summary>
  /// RSDN Data Provider
  /// </summary>
  public abstract class RsdnDataCommonProvider : CacheDataProvider
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
    static RsdnDataCommonProvider()
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

		protected string username = "";
		protected string password = "";

		/// <summary>
		/// Construct RSDN Data Provider
		/// </summary>
    public RsdnDataCommonProvider() : base()
    {
    	encoding = System.Text.Encoding.UTF8;
    }

		public abstract IGroup InternalGetGroup(string groupName);

    public override NewsGroup GetGroup(string groupName)
    {
			IGroup requestedGroup = InternalGetGroup(groupName);
    	currentGroupArticleStartNumber = requestedGroup.FirstArticleNumber;
    	return new NewsGroup(groupName,
				requestedGroup.FirstArticleNumber, requestedGroup.LastArticleNumber,
    		requestedGroup.LastArticleNumber- requestedGroup.FirstArticleNumber + 1,
				true, requestedGroup.Created);
    }

    /// <summary>
    /// Update references cache
    /// </summary>
    /// <param name="message">Message, contains reference information</param>
    /// <returns>The same, original message</returns>
    protected static IArticle UpdateReferences(IArticle message)
    {
    	lock (referenceCache)
    	{
    		referenceCache.AddReference(message.ID, message.ParentID);
				return message;
    	}
    }

		protected abstract IArticle GetArticle(int articleNumber, string groupName);

		protected abstract IArticle GetArticle(int messageID);

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
    	return ToNNTPArticle(
				UpdateReferences(
					GetArticle(articleNumber, groupName)), groupName, content);
    }

		/// <summary>
		/// Regex to extract Message-IDs from rsdn post messages
		/// </summary>
    static protected readonly Regex messageIdNumber =
    	new Regex(@"<(?<messageIdNumber>\d+)@news.rsdn.ru>", RegexOptions.Compiled);

    public override NewsArticle GetNonCachedArticle(string messageID, NewsArticle.Content content)
    {
			try
			{
				int _messageID = int.Parse(messageIdNumber.Match(messageID).Groups["messageIdNumber"].Value);
				IArticle message = GetArticle(_messageID);
				return ToNNTPArticle(UpdateReferences(message), message.Group, content);
			}
			catch (FormatException)
			{
				throw new DataProviderException(DataProviderErrors.NoSuchArticle);
			}
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

		protected abstract IGroup[] GetGroupList(DateTime startTime);

		/// <summary>
		/// Get news groups's descriptions.
		/// </summary>
		/// <param name="startDate">Start date.</param>
		/// <param name="pattern">Match patter for groups' names. Null if none.</param>
		/// <returns></returns>
    public override NewsGroup[] GetGroupList(DateTime startDate, string pattern)
    {
    	IGroup[] groupList = cache["$group_list_cache$"] as IGroup[];
			if (groupList == null)
			{
				groupList = GetGroupList(DateTime.MinValue);
				// add group list to cache with 15 minitues sliding expiration
				cache.Add("$group_list_cache$", groupList, null, Cache.NoAbsoluteExpiration,
					new TimeSpan(0, 15, 0), CacheItemPriority.AboveNormal, null);
			} 
  
			Regex checker = null;
			if (pattern != null)
				checker = new Regex(pattern);

			ArrayList listOfGroups = new ArrayList(groupList.Length);
    	foreach (IGroup currentGroup in groupList)
				if (currentGroup.Created >= startDate &&
						((checker == null) || (checker.IsMatch(currentGroup.Name))))
					listOfGroups.Add(new NewsGroup(currentGroup.Name,
						currentGroup.FirstArticleNumber, currentGroup.LastArticleNumber,
						currentGroup.LastArticleNumber - currentGroup.FirstArticleNumber + 1,
						true, currentGroup.Created));

    	return (NewsGroup[])listOfGroups.ToArray(typeof(NewsGroup));
    }

		protected abstract IArticle[] GetArticleList(string[] groups, DateTime startTime);

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

			IArticle[] articleList = GetArticleList((string[])groups.ToArray(typeof(string)), date);

			ArrayList articles =  new ArrayList();

			// process messages
			foreach (IArticle message in articleList)
				articles.Add(ToNNTPArticle(UpdateReferences(message), message.Group,
					NewsArticle.Content.Header));

			return (NewsArticle[])articles.ToArray(typeof(NewsArticle));
    }

		/// <summary>
		/// Autentificate user.
		/// </summary>
		/// <param name="user">Username.</param>
		/// <param name="pass">Password.</param>
		/// <param name="ip">User's IP.</param>
		/// <returns>User's info. Nulll if user is not authentificated.</returns>
		public abstract IUserInfo InternalAuthentificate(string user, string pass, IPAddress ip);

		/// <summary>
		/// Authentificate user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="ip"></param>
		/// <returns></returns>
    public override bool Authentificate(string user, string pass, IPAddress ip)
    {
			string cacheKey = user + pass;
			// if in cache - auth ok
			IUserInfo userInfo = cache[cacheKey] as IUserInfo;
			if (userInfo != null)
			{
				SetUserInfo(userInfo);
				return true;
			}

			userInfo = InternalAuthentificate(user, pass, ip);
    	if (userInfo != null)
    	{
				// Becasuse of server stores password's hash but not clear password
				// set it to plaint text on client side 
				userInfo.Password = pass;
				// Put user information to cache for 1 hour.
				cache.Add(cacheKey, userInfo, null, Cache.NoAbsoluteExpiration,
					new TimeSpan(1, 0, 0), CacheItemPriority.AboveNormal, null);
				cache.Insert("userId$" + userInfo.ID, userInfo,
					new CacheDependency(null, new string[]{cacheKey}));

				SetUserInfo(userInfo);
				return true;
    	}
    	else
    	{
    		username = "";
    		password = "";
				return false;
    	}
    }

		abstract protected IUserInfo GetUserInfoByID(int id);

		protected IUserInfo GetUserInfo(int id)
		{
			IUserInfo userInfo = cache.Get("userId$" + id) as IUserInfo;
			if (userInfo == null)
			{
				userInfo = GetUserInfoByID(id);
				// Put user information to cache for 1 hour.
				cache.Add("userId$" + userInfo.ID, userInfo, null, Cache.NoAbsoluteExpiration,
					new TimeSpan(1, 0, 0), CacheItemPriority.Normal, null);
			}
			return userInfo;
		}

		/// <summary>
		/// Set data provider's parameters from IUserInfo object.
		/// </summary>
		/// <param name="userInfo"></param>
		protected void SetUserInfo(IUserInfo userInfo)
		{
			username = userInfo.Name;
			password = userInfo.Password;
			if ((rsdnSettings != null) &&
					(rsdnSettings.Formatting == FormattingStyle.UserSettings))
				style = userInfo.MessageFormat;
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

		protected abstract IWebProxy Proxy { get; }

    /// <summary>
    /// Convert rsdn's message to MIME message
    /// Also see rfc 2046, 2112, 2183, 2392, 2557
    /// </summary>
    /// <param name="message"></param>
    /// <param name="newsgroup"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    protected NewsArticle ToNNTPArticle(IArticle message, string newsgroup,
			NewsArticle.Content content)
    {

			//TODO: webproxy!
			NntpTextFormatter formatMessage =
				new NntpTextFormatter(serverName, Proxy, style);

			NewsArticle newsMessage = new NewsArticle("<" + message.ID + message.Postfix + ">",
    		new string[]{newsgroup}, new int[]{message.Number}, content);
    	newsMessage.HeaderEncoding = encoding;

    	if ((content == NewsArticle.Content.Header) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		newsMessage["Path"] = Session.FullHostname + "!not-for-mail";
    		if (message.Author != "")
    			newsMessage.From =
						string.Format("\"{0}\" <{1}@users.rsdn.ru>", message.Author, message.AuthorID);
    		newsMessage.Date = message.Date;
    		newsMessage.Subject = message.Subject;
    		if (message.AuthorID != 0)
    			newsMessage["X-UserID"] = message.AuthorID.ToString();
				newsMessage["X-MessageID"] = message.ID.ToString();
    		
    		// build refences
    		StringBuilder referencesString = new StringBuilder();
    		int[] references;
    		lock (referenceCache)
    		{
    			references = referenceCache.GetReferences(message.ID);
    		}
    		// get message's parents from the start and from the end with limitation of depth
				for (int i = references.Length - 1;
					(i >= references.Length - referencesDeep) && (i > 0); i--)
					referencesString.AppendFormat("<{0}{1}> ", references[i], message.Postfix);
				for (int i = Math.Min(references.Length - referencesDeep - 1, referencesDeep); i > 0; i--)
    			referencesString.AppendFormat("<{0}{1}> ", references[i], message.Postfix);
    		if (referencesString.Length > 0)
    			newsMessage["References"] = referencesString.ToString();
				if (references.Length > 1)
					newsMessage["In-Reply-To"] = string.Format("<{0}{1}> ", references[1], message.Postfix);
    	}

    	if ((content == NewsArticle.Content.Body) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		switch (style)
    		{
    			case FormattingStyle.PlainText :
						StringBuilder plainMessage = new StringBuilder(PrepareText(message.Message));
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
						plainTextBody.Entities.Add(PrepareText(message.Message));
    				plainTextBody.TransferEncoding = ContentTransferEncoding.Base64;
    				plainTextBody.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.WebName);

    				Message htmlTextBody = new Message(false);
						string userType = "";
						if (message.UserType != null && message.UserType.Length > 0)
						{
							userType = string.Format("<span style=\"color: #{0:x6}; font-size: smaller;\">{1}</span>",
								message.UserColor, message.UserType);
						}

						IUserInfo userInfo = GetUserInfo(Format.ToInt(message.AuthorID));

    				string htmlText = string.Format(htmlMessageTemplate, message.AuthorID,
							message.Author, message.GroupID, message.ID,
							formatMessage.Format(message.Message, message.Smile), userType,
							formatMessage.Format(message.HomePage, message.Smile), encoding.WebName,
							Format.ReplaceTags(message.Subject), serverSchemeAndName,
							(message.AuthorID != 0) ?
								string.Format("href='/Users/Profile.aspx?uid={0}'", message.AuthorID) :
								null,
							formatMessage.Format(userInfo == null ? null : userInfo.Origin , true));
    				htmlTextBody.Entities.Add(htmlText);
    				htmlTextBody.TransferEncoding = ContentTransferEncoding.Base64;
    				htmlTextBody.ContentType = string.Format("text/html; charset=\"{0}\"", encoding.WebName);
    				
    				if (formatMessage.ProcessedImagesCount > 0 )
    				{
    					newsMessage.ContentType = "multipart/related; type=\"multipart/alternative\"";

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

		protected abstract IArticle[] GetArticleList(string groupName, int startNumber, int endNumber);

    public override NewsArticle[] GetArticleList(int startNumber, int endNumber, string groupName, NewsArticle.Content content)
    {
			IArticle[] articles = GetArticleList(groupName, startNumber, endNumber);
    	NewsArticle[] newsArticles = new NewsArticle[articles.Length];
   		for (int i = 0; i < articles.Length; i++)
   			newsArticles[i] =
   				ToNNTPArticle(UpdateReferences(articles[i]), groupName, content);

    	return newsArticles;
    }

    /// <summary>
    /// start article number for current group
    /// </summary>
    protected int currentGroupArticleStartNumber = -1;

		protected abstract string PostFile(string filename, string mimeType, byte[] content);

		protected abstract void PostMessage(int mid, string group, string subject, string message);

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
				if (true &&
						(string.Compare(message.ContentTypeType, "multipart", true,
							CultureInfo.InvariantCulture) == 0))
				{
					StringBuilder processedFiles = new StringBuilder();
					foreach (Message entity in message.Entities)
					{
						string disposition = entity.GetHeaderFieldValue("Content-Disposition");
						if ((disposition != null) && (disposition.ToLower().Equals("attachment")))
						{
							string filename =
								entity.GetHeaderFieldParameters("Content-Disposition")["filename"];

							if (filename == null)
							{
								filename = entity.GetHeaderFieldParameters("Content-Type")["name"];
								if (filename == null)
									filename = Guid.NewGuid().ToString();
							}

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

							processedFiles.Append(Util.CRLF).AppendFormat("[url={0}]{1} ({2})[/url]",
								PostFile(filename, string.Format("{0}/{1}",
									entity.ContentTypeType, entity.ContentTypeSubtype), binaryFile),
									filename, Utils.BytesToString(binaryFile.Length));

						}
					}

					if (processedFiles.Length > 0)
					{
						postingText.Append(Util.CRLF).Append(Util.CRLF)
							.Append("Приложенные файлы:").Append(processedFiles);
					}
				}

				// add tagline
				postingText.Append(Util.CRLF).Append("[tagline]Posted via " + Manager.ServerID + "[/tagline]");

				PostMessage(mid, group, Format.Forum.GetEditSubject(message.Subject), postingText.ToString());
			}
			catch (DataProviderException)
			{
				throw;
			}
			catch (System.Exception exception)
			{
				throw new DataProviderException(DataProviderErrors.PostingFailed, exception);
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