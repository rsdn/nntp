using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Rsdn.Nntp.Cache
{
	/// <summary>
	/// Type of cache to use.
	/// </summary>
	public enum CacheType
	{
		/// <summary>
		/// No cache
		/// </summary>
		None,
		/// <summary>
		/// Only in memory cache
		/// </summary>
		Memory,
		/// <summary>
		/// Permanent cache
		/// </summary>
		Persistent
	}

	/// <summary>
	/// Summary description for CacheDataProviderSettings.
	/// </summary>
	[Serializable]
	public class CacheDataProviderSettings
	{
		public CacheDataProviderSettings()
		{
		}

		protected CacheType cacheType = CacheType.None;

		[DefaultValue(CacheType.None)]
		public CacheType Cache
		{
			get { return cacheType; }
			set { cacheType = value; }
		}

		protected TimeSpan absoluteExpiration;

		[XmlElement(DataType = "duration")]
		[Browsable(false)]
		public string Absolute
		{
			get { return absoluteExpiration.ToString(); }
			set { absoluteExpiration = TimeSpan.Parse(value); }
		}

		[XmlIgnore]
		public TimeSpan AbsoluteExpiration
		{
			get { return absoluteExpiration; }
			set { absoluteExpiration = value; }
		}

		protected TimeSpan slidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;

		public TimeSpan SlidingExpiration
		{
			get { return slidingExpiration; }
			set
			{
				if ((value < TimeSpan.Zero) || (value > TimeSpan.FromDays(365)))
					throw new ArgumentOutOfRangeException("SlidingExpiration", value,
						"The SlidingExpiration parameter is set to less than zero or more than one year.");
				slidingExpiration = value;
			}
		}
	}
}
