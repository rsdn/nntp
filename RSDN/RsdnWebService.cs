using System;
using System.Net;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// 
	/// </summary>
	public class RsdnWebService : Rsdn.RsdnNntp.RsdnService.Service
	{
		/// <summary>
		/// RSDN Data Provider's settings
		/// </summary>
		protected DataProviderSettings settings;

		/// <summary>
		/// Construct web-service proxy with specified settings
		/// </summary>
		/// <param name="settings"></param>
		public RsdnWebService(DataProviderSettings settings) : base()
		{
			this.settings = settings;
		}
	
		/// <summary>
		/// To fix bug in .Net Frawework 1.1 with authorization proxy....
		/// </summary>
		protected override WebRequest GetWebRequest(Uri uri)
		{
			HttpWebRequest webRequest = (HttpWebRequest)base.GetWebRequest(uri);
			if (settings.ProxyType == ProxyType.Explicit)
				webRequest.KeepAlive = false;
			return webRequest;
		}
	}
}
