using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using log4net;
using Rsdn.Framework.Formatting;
using Rsdn.Mime;

namespace Rsdn.RsdnNntp
{
  /// <summary>
  /// RSDN NNTP text formatter
  /// </summary>
  public class NntpTextFormatter : TextFormatter
  {
		/// <summary>
		/// Logger 
		/// </summary>
		private static ILog logger =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected string servername;
		protected FormattingStyle formattingStyle;
		protected IWebProxy proxy;

  	public NntpTextFormatter(string servername, IWebProxy proxy, FormattingStyle formattingStyle)
  	{
			this.servername = servername;
			this.formattingStyle = formattingStyle;
			this.proxy = proxy;
  	}

  	public override string CanonicalRsdnHostName
  	{
  		get { return servername; }
  	}

  	NameValueCollection processedImagesIDs = new NameValueCollection();
		protected ArrayList processedImages = new ArrayList();
  	public Message[] GetProcessedImages()
  	{
			return (Message[])processedImages.ToArray(typeof(Message));
  	}
		public int ProcessedImagesCount
		{
			get { return processedImages.Count; }
		}

  	protected override string ProcessImages(Match image)
  	{
			if (formattingStyle == FormattingStyle.HtmlInlineImages)
			{
				WebResponse response = null;
				try
				{
					string imgContentID = processedImagesIDs[image.Groups["url"].Value];
					if (imgContentID == null)
					{
						WebRequest req = WebRequest.Create(image.Groups["url"].Value);
						req.Proxy = proxy;
						response = req.GetResponse();
						Message imgPart = new Message(false);
						imgPart.ContentType = response.ContentType;
						imgContentID = Guid.NewGuid().ToString();
						imgPart["Content-ID"] = '<' + imgContentID + '>';
						imgPart["Content-Location"] = req.RequestUri.ToString();
						imgPart["Content-Disposition"] = "inline";
						imgPart.TransferEncoding = ContentTransferEncoding.Base64;
						using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
						{
							imgPart.Entities.Add(reader.ReadBytes((int)response.ContentLength));
						}
						processedImages.Add (imgPart);
						processedImagesIDs[image.Groups["url"].Value] = imgContentID;
					}
					return string.Format("<img border='0' src='{0}' />",
						"cid:" + imgContentID);
				}
				catch (Exception ex)
				{
					logger.Warn(string.Format("Image {0} not found.", image.Groups["url"].Value), ex);
				}
				finally
				{
					if (response != null)
						response.Close();
				}
			}

			return base.ProcessImages(image);
  	}
  }
}