using System;
using System.Web;
using System.Web.Caching;
using AspCaching = System.Web.Caching;
using System.Collections;

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
		protected static AspCaching.Cache cache = HttpRuntime.Cache;

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
			cache.Add(article.MessageID, article, null, settings.AbsoluteExpiration == TimeSpan.Zero ?
				AspCaching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(settings.AbsoluteExpiration),
				settings.SlidingExpiration, CacheItemPriority.AboveNormal, null);
			CacheDependency dependecy =
				new CacheDependency(null, new string[]{article.MessageID});
			foreach (DictionaryEntry entry in article.MessageNumbers)
			{
				cache.Insert(entry.Key.ToString() + entry.Value.ToString(), article, dependecy);
			}
		}

		#region IDataProvider Members

		/// <summary>
		/// Get article considering cache.
		/// </summary>
		/// <param name="originalMessageID">Message-ID</param>
		/// <param name="content">Necessary content of message.</param>
		/// <returns>Message.</returns>
		public NewsArticle GetArticle(string originalMessageID, NewsArticle.Content content)
		{
			NewsArticle article = null;

			if (settings.Cache != CacheType.None)
			{
				// check message in cache
				article = cache[originalMessageID] as NewsArticle;
				if ((article == null) || !CheckContentSuitable(content, article.Contents))
				// There is no suitable message in cache.
				{
					article = GetNonCachedArticle(originalMessageID, content);

					// Put the message in the cache.
					PutInCache(article);
				}
			}
			else
				// Don't use cache.
				article = GetNonCachedArticle(originalMessageID, content);

			return article;
		}

		public NewsArticle GetArticle(int articleNumber, string groupName, NewsArticle.Content content)
		{
			NewsArticle article = null;

			if (settings.Cache != CacheType.None)
			{
				// check message in cache
				article = cache[groupName + articleNumber.ToString()] as NewsArticle;
				if ((article == null) || !CheckContentSuitable(content, article.Contents))
					// There is no suitable message in cache.
				{
					article = GetNonCachedArticle(articleNumber, groupName, content);

					// Put the message in the cache.
					PutInCache(article);
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

		public abstract NewsArticle[] GetArticleList(DateTime date, string[] patterns);

		public abstract NewsGroup GetGroup(string groupName);

		public abstract NewsGroup[] GetGroupList(DateTime startDate, string pattern);

		public abstract bool Authentificate(string user, string pass);

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