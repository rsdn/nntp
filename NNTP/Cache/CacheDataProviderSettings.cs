using System;
using System.ComponentModel;

namespace Rsdn.Nntp.Cache
{
	/// <summary>
	/// Summary description for CacheDataProviderSettings.
	/// </summary>
	public class CacheDataProviderSettings
	{
		public CacheDataProviderSettings()
		{
		}

		protected bool enabled = false;

		[DefaultValue(false)]
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		protected TimeSpan absoluteExpiration;

		public TimeSpan AbsoluteExpiration
		{
			get { return absoluteExpiration; }
			set { absoluteExpiration = value; }
		}

		protected TimeSpan slidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;

		public TimeSpan SlidingExpiration
		{
			get { return slidingExpiration; }
			set { slidingExpiration = value; }
		}
	}
}
