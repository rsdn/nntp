using System;
using System.Net;
using Rsdn.RsdnNntp.RsdnService;

namespace Rsdn.RsdnNntp.Public.RsdnService
{
	/// <summary>
	/// Summary description for CompressiRsdnService.
	/// </summary>
	public class CompressService : Service2
	{
	  protected override WebRequest GetWebRequest(Uri uri)
	  {
			WebRequest request = base.GetWebRequest(uri);
			request.Headers.Add("Accept-Encoding", "gzip, deflate");
			return request;
	  }

	  protected override WebResponse GetWebResponse(WebRequest request)
	  {
			return new CompressWebResponse(base.GetWebResponse(request));
	  }

	}
}
