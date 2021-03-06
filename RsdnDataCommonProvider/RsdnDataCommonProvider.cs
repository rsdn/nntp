using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Antlr.StringTemplate;
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
		private static readonly ILog logger =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			template = new StringTemplateGroup("format",
				new EmbeddedResourceTemplateLoader(Assembly.GetExecutingAssembly(),
					typeof(TemplateArticle).Namespace),
				null, new TemplateLogger(logger), null);
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
    	var formatter = new BinaryFormatter
      {
      	AssemblyFormat = FormatterAssemblyStyle.Simple
      };
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
    	var formatter = new BinaryFormatter
      {
      	AssemblyFormat = FormatterAssemblyStyle.Simple
      };
    	using (Stream stream = new FileStream(filename, FileMode.Create,
    						FileAccess.Write, FileShare.None))
    	{
    		formatter.Serialize(stream, obj);
    	}		
    }

		/// <summary>
		/// Construct RSDN Data Provider
		/// </summary>
		protected RsdnDataCommonProvider()
		{
    	encoding = Encoding.UTF8;
			formatter = new TextFormatter();
    }

		/// <summary>
		/// Get group by it's name.
		/// </summary>
		/// <param name="groupName">Group name.</param>
		/// <returns>Specified group.</returns>
		public abstract IGroup InternalGetGroup(string groupName);

		/// <summary>
		/// Get group by it's name.
		/// </summary>
		/// <param name="groupName">Group name.</param>
		/// <returns>Specified group.</returns>
    public override NewsGroup GetGroup(string groupName)
    {
			var requestedGroup = InternalGetGroup(groupName);
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
		
		/// <summary>
		/// Get message by group name and number in it.
		/// </summary>
		/// <param name="articleNumber">Message number.</param>
		/// <param name="groupName">Group name.</param>
		/// <returns>Article.</returns>
		protected abstract IArticle GetArticle(int articleNumber, string groupName);

		/// <summary>
		/// Get article by message ID.
		/// </summary>
		/// <param name="messageID">Message ID.</param>
		/// <returns>Article.</returns>
		protected abstract IArticle GetArticle(int messageID);

		/// <summary>
		/// Get article by message ID by it's number in specified group.
		/// </summary>
		/// <param name="articleNumber">Message number.</param>
		/// <param name="groupName">Group name.</param>
		/// <param name="content">Necessary content of message.</param>
		/// <returns>Article.</returns>
    public override NewsArticle GetNonCachedArticle(int articleNumber, string groupName,
			NewsArticle.Content content)
    {
    	return ToNNTPArticle(
				UpdateReferences(
					GetArticle(articleNumber, groupName)), groupName, content);
    }

		/// <summary>
		/// Postfix for using id generation.
		/// </summary>
		static protected readonly string messageIdPostfix = "@news.rsdn.ru";

		/// <summary>
		/// Regex to extract Message-IDs from rsdn post messages
		/// </summary>
    static protected readonly Regex messageIdNumber =
    	new Regex(string.Format(@"<(?<messageIdNumber>\d+){0}>", messageIdPostfix),
				RegexOptions.Compiled);

		/// <summary>
		/// Get article without lookup in cache by message id.
		/// </summary>
		/// <param name="messageID">Message ID.</param>
		/// <param name="content">Necessary content of message.</param>
		/// <returns>Article.</returns>
		public override NewsArticle GetNonCachedArticle(string messageID, NewsArticle.Content content)
    {
			try
			{
				var _messageID = int.Parse(messageIdNumber.Match(messageID).Groups["messageIdNumber"].Value);
				var message = GetArticle(_messageID);
				return ToNNTPArticle(UpdateReferences(message), message.Group, content);
			}
			catch (FormatException)
			{
				throw new DataProviderException(DataProviderErrors.NoSuchArticle);
			}
    }

		/// <summary>
		/// Get next article IDs.
		/// Only article number &amp; MessageID required.
		/// </summary>
		/// <param name="messageNumber">Current message number.</param>
		/// <param name="groupName">Current news group.</param>
		/// <returns>Next article's IDs.</returns>
    public override NewsArticle GetNextArticle(int messageNumber, string groupName)
    {
    	var articleList = GetArticleList(messageNumber + 1, int.MaxValue,
    		groupName, NewsArticle.Content.Header);
  
    	if (articleList.Length == 0)
    		throw new DataProviderException(DataProviderErrors.NoNextArticle);

    	return articleList[0];
    }

		/// <summary>
		/// Get previous article IDs.
		/// Only article number &amp; MessageID required.
		/// </summary>
		/// <param name="messageNumber">Current message number.</param>
		/// <param name="groupName">Current news group.</param>
		/// <returns>Previous article's IDs.</returns>
    public override NewsArticle GetPrevArticle(int messageNumber, string groupName)
    {
    	var articleList = GetArticleList(currentGroupArticleStartNumber,
    		messageNumber - 1,	groupName, NewsArticle.Content.Header);
  
    	if (articleList.Length == 0)
    		throw new DataProviderException(DataProviderErrors.NoPrevArticle);

    	return articleList[articleList.Length - 1];
    }

		/// <summary>
		/// Get list of groups created after specified date.
		/// </summary>
		/// <param name="startTime">Start time.</param>
		/// <returns>List of groups.</returns>
		protected abstract IGroup[] GetGroupList(DateTime startTime);

		/// <summary>
		/// Get news groups's descriptions.
		/// </summary>
		/// <param name="startDate">Start date.</param>
		/// <param name="pattern">Match patter for groups' names. Null if none.</param>
		/// <returns></returns>
    public override NewsGroup[] GetGroupList(DateTime startDate, string pattern)
    {
    	var groupList = GetGroupList(DateTime.MinValue);
  
			Regex checker = null;
			if (pattern != null)
				checker = new Regex(pattern);

			var listOfGroups = new List<NewsGroup>(groupList.Length);
    	foreach (var currentGroup in groupList)
				if (currentGroup.Created >= startDate &&
						((checker == null) || (checker.IsMatch(currentGroup.Name))))
					listOfGroups.Add(new NewsGroup(currentGroup.Name,
						currentGroup.FirstArticleNumber, currentGroup.LastArticleNumber,
						currentGroup.LastArticleNumber - currentGroup.FirstArticleNumber + 1,
						true, currentGroup.Created));

    	return listOfGroups.ToArray();
    }

		/// <summary>
		/// Get list of articles by array of group names and start date from which retrieve messages.
		/// </summary>
		/// <param name="groups">Group names.</param>
		/// <param name="startTime">Start time.</param>
		/// <returns>List of articles.</returns>
		protected abstract IArticle[] GetArticleList(string[] groups, DateTime startTime);

		/// <summary>
		/// Get article list.
		/// </summary>
		/// <param name="date">Start date.</param>
		/// <param name="pattern">Group name pattern.</param>
		/// <returns>List of articles.</returns>
    public override NewsArticle[] GetArticleList(DateTime date, string pattern)
    {
			var groups = new List<string>();

			// get all appropriated groups
			foreach (var group in GetGroupList(DateTime.MinValue, pattern))
				groups.Add(group.Name);

			var articleList = GetArticleList(groups.ToArray(), date);

      var articles = new List<NewsArticle>();

			// process messages
			foreach (var message in articleList)
				articles.Add(ToNNTPArticle(UpdateReferences(message), message.Group,
					NewsArticle.Content.Header));

			return articles.ToArray();
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
			var cacheKey = user + pass;
			// if in cache - auth ok
			var userInfo = cache[cacheKey] as IUserInfo;
			if (userInfo != null)
			{
				SetUserInfo(userInfo);
				return true;
			}

			userInfo = InternalAuthentificate(user, pass, ip);
			if (userInfo == null)
			{
				return false;
			}

			// Put user information to cache for 1 hour.
			cache.Add(cacheKey, userInfo, null, Cache.NoAbsoluteExpiration,
			          new TimeSpan(1, 0, 0), CacheItemPriority.AboveNormal, null);
			cache.Insert("userId$" + userInfo.ID, userInfo,
			             new CacheDependency(null, new[]{cacheKey}));

			SetUserInfo(userInfo);
			return true;
    }

		/// <summary>
		/// Get user info by it's id without lookup to cache.
		/// </summary>
		/// <param name="id">User ID.</param>
		/// <returns>User info.</returns>
		abstract protected IUserInfo GetUserInfoByID(int id);

		/// <summary>
		/// Get user info by it's id with lookup to cache.
		/// </summary>
		/// <param name="id">User ID.</param>
		/// <returns>User info.</returns>
		protected IUserInfo GetUserInfo(int id)
		{
			var userInfo = cache.Get("userId$" + id) as IUserInfo;
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
		protected virtual void SetUserInfo(IUserInfo userInfo)
		{
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

		/// <summary>
		/// Proxy used to retrieve external resources.
		/// </summary>
		protected abstract IWebProxy Proxy { get; }

		/// <summary>
		/// RSDN text formatter.
		/// </summary>
		protected TextFormatter formatter;

		/// <summary>
		/// Image processor
		/// </summary>
		protected ImageProcessor imageProcessor;

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
			var newsMessage =
				new NewsArticle(string.Format("<{0}{1}>", message.ID, message.Postfix),
    			new[]{newsgroup}, new[]{message.Number}, content)
				{ HeaderEncoding = encoding };

    	if ((content == NewsArticle.Content.Header) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		newsMessage["Path"] = Session.FullHostname + "!not-for-mail";
				newsMessage.From =string.Format("\"{0}\" <{1}@users.rsdn.ru>",
					string.IsNullOrEmpty(message.Author) ? "������ " + message.UserType : message.Author,
					message.AuthorID);
					
    		newsMessage.Date = message.Date;
    		newsMessage.Subject = message.Subject;
    		if (message.AuthorID != 0)
    			newsMessage["X-UserID"] = message.AuthorID.ToString();
				newsMessage["X-MessageID"] = message.ID.ToString();
    		
				if (!string.IsNullOrEmpty(message.Tags))
				{
					newsMessage["Keywords"] = message.Tags;	
				}

    		// build refences
    		var referencesString = new StringBuilder();
    		int[] references;
    		lock (referenceCache)
    		{
    			references = referenceCache.GetReferences(message.ID);
    		}
    		// get message's parents from the start and from the end with limitation of depth
				for (var i = references.Length - 1;
					(i >= references.Length - referencesDeep) && (i > 0); i--)
					referencesString.AppendFormat("<{0}{1}> ", references[i], message.Postfix);
				for (var i = Math.Min(references.Length - referencesDeep - 1, referencesDeep); i > 0; i--)
    			referencesString.AppendFormat("<{0}{1}> ", references[i], message.Postfix);
				if (referencesString.Length > 0)
				{
					// remove last space
					referencesString.Length--;
					newsMessage["References"] = referencesString.ToString();
				}
				if (references.Length > 1)
					newsMessage["In-Reply-To"] = string.Format("<{0}{1}>", references[1], message.Postfix);
    	}

    	if ((content == NewsArticle.Content.Body) ||
    		(content == NewsArticle.Content.HeaderAndBody))
    	{
    		switch (style)
    		{
    			case FormattingStyle.PlainText :
						var plainMessage = new StringBuilder(PrepareText(message.Message));
    				newsMessage.Entities.Add(plainMessage.ToString());
    				newsMessage.TransferEncoding = ContentTransferEncoding.Base64;
    				newsMessage.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.WebName);
    				break;
    			case FormattingStyle.Html :
    			case FormattingStyle.HtmlInlineImages :
    				var plainTextBody = new Message(false);
						plainTextBody.Entities.Add(PrepareText(message.Message));
    				plainTextBody.TransferEncoding = ContentTransferEncoding.Base64;
    				plainTextBody.ContentType = string.Format("text/plain; charset=\"{0}\"", encoding.WebName);

    				var htmlTextBody = new Message(false);
						var userInfo = GetUserInfo(Format.ToInt(message.AuthorID));

						if (imageProcessor != null)
							imageProcessor.ClearProcessedImages();

						var htmlMessage = template.GetInstanceOf("HtmlEmailTemplate");
						htmlMessage.SetAttribute("message", new TemplateArticle(message));
						htmlMessage.SetAttribute("text",
							formatter.Format(message.Message, message.Smile));
						htmlMessage.SetAttribute("homepage",
							formatter.Format(message.HomePage, message.Smile));
						htmlMessage.SetAttribute("origin",
							formatter.Format(userInfo == null ? null : userInfo.Origin, true));
						htmlMessage.SetAttribute("encoding", encoding);
						htmlMessage.SetAttribute("subject", Format.ReplaceTags(message.Subject));
						htmlMessage.SetAttribute("server", serverSchemeAndName);
						htmlMessage.SetAttribute("replyMarker", htmlReplyMarker);

    				htmlTextBody.Entities.Add(htmlMessage.ToString());
    				htmlTextBody.TransferEncoding = ContentTransferEncoding.Base64;
    				htmlTextBody.ContentType = string.Format("text/html; charset=\"{0}\"", encoding.WebName);
    				
    				if (imageProcessor != null && imageProcessor.ProcessedImagesCount > 0 )
    				{
    					newsMessage.ContentType = "multipart/related; type=\"multipart/alternative\"";

    					var combineMessage = new Message(false)
								{ ContentType = "multipart/alternative" };
    					combineMessage.Entities.Add(plainTextBody);
    					combineMessage.Entities.Add(htmlTextBody);
    					newsMessage.Entities.Add(combineMessage);

    					foreach (var img in imageProcessor.GetProcessedImages())
   							newsMessage.Entities.Add(img);

							imageProcessor.ClearProcessedImages();
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

		/// <summary>
		/// Get list of articles by group name, start and end number of messages in it.
		/// </summary>
		/// <param name="groupName">Group name.</param>
		/// <param name="startNumber">Start message number.</param>
		/// <param name="endNumber">End message number.</param>
		/// <returns>List of articles.</returns>
		protected abstract IArticle[] GetArticleList(string groupName, int startNumber, int endNumber);

		/// <summary>
		/// Get list of articles by group name, start and end number of messages in it.
		/// Retrieve only specified content of messages.
		/// </summary>
		/// <param name="groupName">Group name.</param>
		/// <param name="startNumber">Start message number.</param>
		/// <param name="endNumber">End message number.</param>
		/// <param name="content">Necessary content of articles.</param>
		/// <returns>List of articles.</returns>
		public override NewsArticle[] GetArticleList(int startNumber, int endNumber, string groupName, NewsArticle.Content content)
    {
			var articles = GetArticleList(groupName, startNumber, endNumber);
    	var newsArticles = new NewsArticle[articles.Length];
   		for (var i = 0; i < articles.Length; i++)
   			newsArticles[i] =
   				ToNNTPArticle(UpdateReferences(articles[i]), groupName, content);

    	return newsArticles;
    }

    /// <summary>
    /// start article number for current group
    /// </summary>
    protected int currentGroupArticleStartNumber = -1;

		/// <summary>
		/// Post file to user's storage.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="mimeType">Content type of file.</param>
		/// <param name="content">Binary content of file.</param>
		/// <returns>Posted file name on the server.</returns>
		protected abstract string PostFile(string filename, string mimeType, byte[] content);

		/// <summary>
		/// Post message.
		/// </summary>
		/// <param name="mid">Parent message id, if it's reply.</param>
		/// <param name="group">Group name to which post message.</param>
		/// <param name="subject">Message's subject.</param>
		/// <param name="message">Message's content.</param>
		/// <param name="message">Message's tags.</param>
		protected abstract void PostMessage(int mid, string group, string subject, string message, string tags);

		/// <summary>
		/// Marker to determine reply in html format.
		/// </summary>
		protected readonly string htmlReplyMarker = "DFA0BD11-EE3D-4DB4-98D5-FC2BDA095E3C";

    /// <summary>
    /// Post MIME message through data provider
    /// </summary>
    /// <param name="message"></param>
    public override void PostMessage(Message message)
    {
			try
			{
				var postingText = GetPlainTextFromMessage(message);
				if (postingText.Length == 0)
					throw new DataProviderException(DataProviderErrors.PostingFailed, "Empty message.");

				// get message ID
				var mid = 0;
				if (message["References"] != null)
					foreach (Match messageIDMatch in messageIdNumber.Matches(message["References"]))
						mid = int.Parse(messageIDMatch.Groups["messageIdNumber"].Value);
				// get posting news group
				var group = message["Newsgroups"].Split(new[]{','}, 2)[0].Trim();
    		
				// process attachments
				if (true &&
						"multipart".Equals(message.ContentTypeType, StringComparison.OrdinalIgnoreCase))
				{
					var processedFiles = new StringBuilder();
					foreach (Message entity in message.Entities)
					{
						var disposition = entity.GetHeaderFieldValue("Content-Disposition");
						if (!string.IsNullOrEmpty(disposition) &&
								("attachment".Equals(disposition, StringComparison.OrdinalIgnoreCase) ||
									"inline".Equals(disposition, StringComparison.OrdinalIgnoreCase)))
						{
							var filename =
								entity.GetHeaderFieldParameters("Content-Disposition")["filename"]
								?? entity.GetHeaderFieldParameters("Content-Type")["name"]
								?? Guid.NewGuid().ToString();

							// post file ....
							var binaryFile = new byte[0];
							foreach (var body in entity.Entities)
							{
								byte[] binaryBody;
								if (body is byte[])
								{
									binaryBody = (byte[])body;
								}
								else
									binaryBody = entity.Encoding.GetBytes(body.ToString());
								var newBinaryFile = new byte[binaryFile.Length + binaryBody.Length];
								Buffer.BlockCopy(binaryFile, 0, newBinaryFile, 0, binaryFile.Length);
								Buffer.BlockCopy(binaryBody, 0, newBinaryFile, binaryFile.Length, binaryBody.Length);
								binaryFile = newBinaryFile;
							}

							processedFiles.Append(Util.CRLF)
								.AppendFormat(new FileSizeFormatProvider(),
									"[url={0}]{1} ({2:fs})[/url]",
									PostFile(filename, string.Format("{0}/{1}",
										entity.ContentTypeType, entity.ContentTypeSubtype), binaryFile),
										filename, binaryFile.Length);
						}
					}

					if (processedFiles.Length > 0)
					{
						postingText.Append(Util.CRLF).Append(Util.CRLF)
							.Append("����������� �����:").Append(processedFiles);
					}
				}

				// add tagline
				postingText.Append(Util.CRLF).Append("[tagline]Posted via " + Manager.ServerID + "[/tagline]");

				var postingTextString = postingText.ToString();

				if (postingTextString.IndexOf(htmlReplyMarker) >= 0)
					throw new DataProviderException(DataProviderErrors.PostingFailed,
						"Reply only in plain text not html. For details see http://www.rsdn.ru/projects/rsdnnntp/rsdnnntp.xml.");

				PostMessage(mid, group, Format.Forum.GetEditSubject(message.Subject), postingTextString, message["Keywords"]);
			}
			catch (DataProviderException)
			{
				throw;
			}
			catch (Exception exception)
			{
				throw new DataProviderException(DataProviderErrors.PostingFailed, exception);
			}	
    }

  	/// <summary>
		/// Templates to format message
		/// </summary>
    protected static readonly StringTemplateGroup template;
		/// <summary>
		/// Message encoding
		/// </summary>
    protected Encoding encoding;

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
		/// Server address (scheme &amp; host name) used to generate site links.
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

				imageProcessor = (style == FormattingStyle.HtmlInlineImages) ?
					new ImageProcessor(messageIdPostfix, rsdnSettings.MaxImagesSize, Proxy) : null;
    	}

			if (imageProcessor != null)
				formatter = new TextFormatter(imageProcessor.ProcessImagesDelegate);
    }

		/// <summary>
		/// Detect platform specific line breaks
		/// </summary>
		protected static Regex platformDependedBreak = new Regex(@"(?<!\r)\n");

	  /// <summary>
    /// remove unnecessary tags (tagline, moderator)
    /// </summary>
    protected static string PrepareText(string text)
    {
    	if (text == null)
    		return "";
    	
  		return platformDependedBreak.Replace(
				TextFormatter.RemoveTaglineTag(text), Util.CRLF);
    }

		/// <summary>
		/// Get necessary configuration object type
		/// </summary>
		/// <returns></returns>
    public override Type GetConfigType()
    {
    	return typeof(DataProviderSettings);
    }
	  
	/// <summary>
    /// Get only plain text from MIME message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static StringBuilder GetPlainTextFromMessage(Message message)
    {
    	var text = new StringBuilder();
    	if ("text".Equals(message.ContentTypeType, StringComparison.OrdinalIgnoreCase) &&
					("plain".Equals(message.ContentTypeSubtype, StringComparison.OrdinalIgnoreCase)) ||
						"multipart".Equals(message.ContentTypeSubtype, StringComparison.OrdinalIgnoreCase))
    		foreach (var entity in message.Entities)
    			if (entity is Message)
    				text.Append(GetPlainTextFromMessage((Message)entity));
    			else
    				text.Append(entity);
    	return text;
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
		public override NewsArticle GetArticle(string originalMessageID, NewsArticle.Content content)
		{
			return base.GetArticle (originalMessageID, content, style.ToString());
		}
	}
}