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
	}

	/// <summary>
	/// Summary description for CacheDataProviderSettings.
	/// </summary>
	[Serializable]
	public class CacheDataProviderSettings
	{
		protected CacheType cacheType = CacheType.None;

		[DefaultValue(CacheType.None)]
		public CacheType Cache
		{
			get { return cacheType; }
			set { cacheType = value; }
		}

		protected TimeSpan absoluteExpiration = TimeSpan.Zero;

		[XmlElement(DataType = "duration")]
		[Browsable(false)]
		public string Absolute
		{
			get { return AbsoluteExpiration.ToString(); }
			set { AbsoluteExpiration = TimeSpan.Parse(value); }
		}

		[XmlIgnore]
		public TimeSpan AbsoluteExpiration
		{
			get { return absoluteExpiration; }
			set
			{
				if ((value != TimeSpan.Zero) && (slidingExpiration != TimeSpan.Zero))
					throw new ArgumentException("Only the one of time parameters may be specified.", "AbsoluteExpiration");

				absoluteExpiration = value;
			}
		}

		protected TimeSpan slidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;

		[XmlElement(DataType = "duration")]
		[Browsable(false)]
		public string Sliding
		{
			get { return SlidingExpiration.ToString(); }
			set { SlidingExpiration = TimeSpan.Parse(value); }
		}

		[XmlIgnore]
		public TimeSpan SlidingExpiration
		{
			get { return slidingExpiration; }
			set
			{
				if ((value < TimeSpan.Zero) || (value > TimeSpan.FromDays(365)))
					throw new ArgumentOutOfRangeException("SlidingExpiration", value,
						"The SlidingExpiration parameter is set to less than zero or more than one year.");

				if ((value != TimeSpan.Zero) && (absoluteExpiration != TimeSpan.Zero))
					throw new ArgumentException("Only the one of time parameters may be specified.", "SlidingExpiration");

				slidingExpiration = value;
			}
		}
	}
}
