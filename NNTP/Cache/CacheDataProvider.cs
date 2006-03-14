using System;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;
using System.Net;

using Rsdn.Nntp;

namespace Rsdn.Nntp.Cache
{
	/// <summary>
	/// Data provider with cache ability.
	/// </summary>
	public abstract class CacheDataProvider : IDataProvider
	{
		/// <summary>
		/// Cache settings object.
		/// </summary>
		protected CacheDataProviderSettings settings = new CacheDataProviderSettings();

		/// <summary>
		/// Cache storage object.
		/// </summary>
    protected static System.Web.Caching.Cache cache = HttpRuntime.Cache;

		/// <summary>
		/// Check if current content of article is suitable for need content.
		/// </summary>
		/// <param name="need">Necessary content of the message.</param>
		/// <param name="current">Current content if the message.</param>
		/// <returns></returns>
		protected static bool CheckContentSuitable(NewsArticle.Content need, NewsArticle.Content current)
		{
			bool suit = false;

			switch (need)
			{
				case NewsArticle.Content.None : 
					switch (current)
					{
						case NewsArticle.Content.None :
						case NewsArticle.Content.Header :
						case NewsArticle.Content.HeaderAndBody :
							suit = true;
							break;
					}
					break;
				case NewsArticle.Content.Header :
					switch (current)
					{
						case NewsArticle.Content.Header :
						case NewsArticle.Content.HeaderAndBody :
							suit = true;
							break;
					}
					break;
				case NewsArticle.Content.Body : 
					switch (current)
					{
						case NewsArticle.Content.Body :
						case NewsArticle.Content.HeaderAndBody :
							suit = true;
							break;
					}
					break;
				case NewsArticle.Content.HeaderAndBody :
					if (current == NewsArticle.Content.HeaderAndBody)
						suit = true;
					break;
			}		

			return suit;
		}

		/// <summary>
		/// Put the message in cache.
		/// </summary>
		/// <param name="article">Article.</param>
		protected void PutInCache(NewsArticle article)
		{
			PutInCache(article, null);
		}

		/// <summary>
		/// Put the message in cache considering cache salt.
		/// </summary>
		/// <param name="article">Article.</param>
		/// <param name="cacheSalt">Additional cache managing parameter.</param>
		protected void PutInCache(NewsArticle article, string cacheSalt)
		{
			cache.Add(article.MessageID + cacheSalt, article, null,
				settings.AbsoluteExpiration == TimeSpan.Zero ?
          System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(settings.AbsoluteExpiration),
				settings.SlidingExpiration, CacheItemPriority.AboveNormal, null);
			CacheDependency dependecy =
				new CacheDependency(null, new string[]{article.MessageID + cacheSalt});
			foreach (KeyValuePair<string, int> entry in article.MessageNumbers)
			{
				cache.Insert(entry.Key + entry.Value.ToString() + cacheSalt,
					article, dependecy);
			}
		}

		#region IDataProvider Members

		/// <summary>
		/// Get article considering cache.
		/// </summary>
		/// <param name="originalMessageID">Message-ID</param>
		/// <param name="content">Necessary content of message.</param>
		/// <returns>Message.</returns>
		public virtual NewsArticle GetArticle(string originalMessageID, NewsArticle.Content content)
		{
			return GetArticle(originalMessageID, content, null);
		}

		/// <summary>
		/// Get article considering cache with additional cache managing parameter (caching salt).
		/// </summary>
		/// <param name="originalMessageID">Message-ID</param>
		/// <param name="content">Necessary content of message.</param>
		/// <param name="cacheSalt">Additional cache managing parameter.</param>
		/// <returns>Message.</returns>
		public virtual NewsArticle GetArticle(string originalMessageID, NewsArticle.Content content,
			string cacheSalt)
		{
			NewsArticle article = null;

			if (settings.Cache != CacheType.None)
			{
				// check message in cache
				article = cache[originalMessageID + cacheSalt] as NewsArticle;
				if ((article == null) || !CheckContentSuitable(content, article.Contents))
				// There is no suitable message in cache.
				{
					article = GetNonCachedArticle(originalMessageID, content);

					// Put the message in the cache.
					PutInCache(article, cacheSalt);
				}
			}
			else
				// Don't use cache.
				article = GetNonCachedArticle(originalMessageID, content);

			return article;
		}

		/// <summary>
		/// Get article considering cache.
		/// </summary>
		/// <param name="articleNumber"></param>
		/// <param name="groupName"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public virtual NewsArticle GetArticle(int articleNumber, string groupName,
			NewsArticle.Content content)
		{
			return GetArticle(articleNumber, groupName, content, null);
		}

		/// <summary>
		/// Get article considering cache with additional cache managing parameter (caching salt).
		/// </summary>
		/// <param name="articleNumber"></param>
		/// <param name="groupName"></param>
		/// <param name="content"></param>
		/// <param name="cacheSalt"></param>
		/// <returns></returns>
		public virtual NewsArticle GetArticle(int articleNumber, string groupName,
			NewsArticle.Content content, string cacheSalt)
		{
			NewsArticle article = null;

			if (settings.Cache != CacheType.None)
			{
				// check message in cache
				article = cache[groupName + articleNumber.ToString() + cacheSalt] as NewsArticle;
				if ((article == null) || !CheckContentSuitable(content, article.Contents))
					// There is no suitable message in cache.
				{
					article = GetNonCachedArticle(articleNumber, groupName, content);

					// Put the message in the cache.
					PutInCache(article, cacheSalt);
				}
			}
			else
				// Don't use cache.
				article = GetNonCachedArticle(articleNumber, groupName, content);

			return article;
		}

		public abstract NewsArticle[] GetArticleList(int startNumber, int endNumber, string groupName,
			NewsArticle.Content content);

		public virtual Type GetConfigType()
		{
			return typeof(CacheDataProviderSettings);
		}

		/// <summary>
		/// Configure data provider.
		/// </summary>
		/// <param name="settings"></param>
		public virtual void Config(object settings)
		{
			if (settings is CacheDataProviderSettings)
				this.settings = (CacheDataProviderSettings)settings;
		}

		public abstract NewsArticle GetNextArticle(int messageNumber, string groupName);

		public abstract NewsArticle GetPrevArticle(int messageNumber, string groupName);

		public abstract NewsArticle[] GetArticleList(DateTime date, string pattern);

		public abstract NewsGroup GetGroup(string groupName);

		public abstract NewsGroup[] GetGroupList(DateTime startDate, string pattern);

		public abstract bool Authentificate(string user, string pass, IPAddress ip);

		public abstract void PostMessage(Rsdn.Mime.Message article);

		public abstract string Identity
		{
			get;
		}

		public abstract bool PostingAllowed
		{
			get;
		}

		public abstract Session.States InitialSessionState
		{
			get;
		}

		public abstract bool WantArticle(string messageID);

		#endregion

		#region IDisposable Members

		public abstract void Dispose();

		#endregion

		#region Non cached members
		
		public abstract NewsArticle GetNonCachedArticle(string originalMessageID,
			NewsArticle.Content content);
		
		public abstract NewsArticle GetNonCachedArticle(int articleNumber, string groupName,
			NewsArticle.Content content);
		
		#endregion
	}
}