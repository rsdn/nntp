using System;
using System.Net;
using System.IO;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Rsdn.RsdnNntp.RsdnService
{
	/// <summary>
	/// Summary description for CompressWebResponse.
	/// </summary>
	public class CompressWebResponse : WebResponse
	{
		WebResponse response;

		public CompressWebResponse(WebResponse response)
		{
			this.response = response;
		}

	  public override Stream GetResponseStream ()
	  {
			Stream responseStream;

			switch (response.Headers["Content-Encoding"])
			{
				case "gzip" :
					responseStream = new GZipInputStream(response.GetResponseStream());
					break;
				case "deflate" :
					responseStream =
						new InflaterInputStream(response.GetResponseStream(), new Inflater(true));
					break;
				default :
					responseStream = response.GetResponseStream();
					break;
			}
			return responseStream;
	  }

	  public override WebHeaderCollection Headers
	  {
	    get { return response.Headers; }
	  }

	  public override Uri ResponseUri
	  {
	    get { return response.ResponseUri; }
	  }

	  public override string ContentType
	  {
			get { return response.ContentType; }
	    set { response.ContentType = value; }
	  }

	  public override long ContentLength
	  {
	    get { return response.ContentLength; }
	    set { response.ContentLength = value; }
	  }
	}
}
