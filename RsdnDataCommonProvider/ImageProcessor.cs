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
  /// Image processor
  /// </summary>
  public class ImageProcessor
  {
		/// <summary>
		/// Logger 
		/// </summary>
		private static ILog logger =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Web Proxy used to retrieve external resources.
		/// </summary>
		protected IWebProxy proxy;

		/// <summary>
		/// Message text formatter used to format nntp messages.
		/// </summary>
		/// <param name="proxy">Proxy to retrieve extrenal resources.</param>
  	public ImageProcessor(IWebProxy proxy)
  	{
			this.proxy = proxy;
			ProcessImagesDelegate = new TextFormatter.ProcessImagesDelegate(ProcessImages);
  	}

  	NameValueCollection processedImagesIDs = new NameValueCollection();
		ArrayList processedImages = new ArrayList();

		/// <summary>
		/// Array of processed during message formatting inline images.
		/// </summary>
		/// <returns></returns>
  	public Message[] GetProcessedImages()
  	{
			return (Message[])processedImages.ToArray(typeof(Message));
  	}
		/// <summary>
		/// Number of processed inline images.
		/// </summary>
		public int ProcessedImagesCount
		{
			get { return processedImages.Count; }
		}

		/// <summary>
		/// Process image specified through [img] tag.
		/// </summary>
		/// <param name="formatter">Owning formatter.</param>
		/// <param name="image">Iamge tag match.</param>
		/// <returns>Processed tag.</returns>
  	protected string ProcessImages(TextFormatter formatter, Match image)
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

			return formatter.ProcessImages(image);
  	}

		/// <summary>
		/// Delegate to process images.
		/// </summary>
		public TextFormatter.ProcessImagesDelegate ProcessImagesDelegate;

	}
}