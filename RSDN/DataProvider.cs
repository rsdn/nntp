using System;
using System.Collections.Specialized;
using System.Globalization;
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
using Rsdn.RsdnNntp.Common;
using Rsdn.RsdnNntp.Public.RsdnService;
using Rsdn.RsdnNntp.RsdnService;

namespace Rsdn.RsdnNntp.Public
{
  /// <summary>
  /// RSDN Data Provider
  /// </summary>
  public class RsdnDataPublicProvider : RsdnDataCommonProvider
  {
		/// <summary>
		/// Construct RSDN Data Provider
		/// </summary>
    public RsdnDataPublicProvider() : base()
    {
    	webService = new Service();
    }

    /// <summary>
    /// RSDN forums' web-service proxy
    /// </summary>
    protected Service webService;

  	public override IGroup InternalGetGroup(string groupName)
  	{
			try
			{
				group requestedGroup = webService.GroupInfo(groupName, username, password);
				if (requestedGroup.error != null)
					ProcessErrorMessage(requestedGroup.error);
				return new Group(requestedGroup);
			}		
			catch (System.Exception exception)
			{
				try
				{
					ProcessException(exception);
					return null;
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
  	}

  	protected override IGroup[] GetGroupList(DateTime startTime)
  	{
			try
			{
				group_list groupList =
					webService.GetGroupList(username, password, startTime);
				if (groupList.error != null)
					ProcessErrorMessage(groupList.error);
				Group[] groups = new Group[groupList.groups.Length];
				for (int i = 0; i < groups.Length; i++)
					groups[i] = new Group(groupList.groups[i]);
				return groups;
			}				
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
			}	
  	}

  	protected override IArticle GetArticle(int articleNumber, string groupName)
  	{
			try
			{
				article message =
					webService.GetArticle(groupName, articleNumber, username,	password);
				if (message.error != null)
					ProcessErrorMessage(message.error);
				return new Article(message);
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
			}	
  	}

  	protected override IArticle GetArticle(int messageID)
  	{
			try
			{
				article message = webService.GetArticleByID(messageID , username,	password);
				if (message.error != null)
					ProcessErrorMessage(message.error);
				return new Article(message);
			}	
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
			}	
		}

  	/// <summary>
  	/// Autentificate user.
  	/// </summary>
  	/// <param name="user">Username.</param>
  	/// <param name="pass">Password.</param>
  	/// <param name="ip">User's IP.</param>
  	/// <returns>User's info. Nulll if user is not authentificated.</returns>
  	public override IUserInfo InternalAuthentificate(string user, string pass, IPAddress ip)
  	{
			if (Utils.InvalidXmlCharacters.IsMatch(user))
				throw new DataProviderException(DataProviderErrors.NoPermission,
					"Username contains not allowed symbols.");
			if (Utils.InvalidXmlCharacters.IsMatch(pass))
				throw new DataProviderException(DataProviderErrors.NoPermission,
					"Password contains not allowed symbols.");

			if (webService.Authentication(user, pass).ok)
			{
				return new UserInfo(webService.GetUserInfo(user, pass));
			}
			else
				return null;
  	}

  	protected override IUserInfo GetUserInfoByID(int id)
  	{
			try
			{
				return new UserInfo(webService.GetUserInfoByID(username, password, id));
			}			
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
			}
  	}

  	protected override IArticle[] GetArticleList(string groupName, int startNumber, int endNumber)
  	{
			try
			{
		    article_list articleList =
					webService.ArticleList(groupName, startNumber, endNumber, username, password);
				if (articleList.error != null)
					ProcessErrorMessage(articleList.error);
				// sometimes web-service return null....
				if (articleList != null)
				{
					IArticle[] iArticles = new IArticle[articleList.articles.Length];
					for (int i = 0; i < iArticles.Length; i++)
						iArticles[i] = new Article(articleList.articles[i]);
					return iArticles;
				}
				else
					return new Article[0];
    	}
			catch (System.Exception exception)
			{
			  ProcessException(exception);
				return null;
			}	
  	}

  	protected override IArticle[] GetArticleList(string[] groups, DateTime startTime)
  	{
			try
			{
				article_list articleList =
					webService.ArticleListFromDate(groups, startTime, username, password);
				if (articleList.error != null)
					ProcessErrorMessage(articleList.error);
				Article[] iArticles = new Article[articleList.articles.Length];
				for (int i = 0; i < iArticles.Length; i++)
					iArticles[i] = new Article(articleList.articles[i]);
				return iArticles;
			}
			catch (System.Exception exception)
			{
				ProcessException(exception);
				return null;
			}	
  	}

  	protected override string PostFile(string filename, string mimeType, byte[] content)
  	{
			return webService.PostFile(filename, mimeType, content,
				username, password);
  	}

  	protected override void PostMessage(int mid, string group, string subject, string message)
  	{
			post_result result = 
				webService.PostMessage(username, password, mid, group,
				Utils.ProcessInvalidXmlCharacters(subject),
				Utils.ProcessInvalidXmlCharacters(message));

			if (!result.ok)
				throw new DataProviderException(DataProviderErrors.PostingFailed, result.error);
  	}

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
  	/// Get necessary configuration object type
  	/// </summary>
  	/// <returns></returns>
  	public override System.Type GetConfigType()
  	{
  		return typeof(DataProviderSettings);
  	}

  	protected override IWebProxy Proxy
  	{
  		get { return webService.Proxy; }
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
    	}
    }
	}
}