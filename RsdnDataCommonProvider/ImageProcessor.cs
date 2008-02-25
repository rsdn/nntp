using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
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
		private static readonly ILog logger =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Web Proxy used to retrieve external resources.
		/// </summary>
		protected IWebProxy proxy;

		/// <summary>
		/// Images' Content-ID postfix
		/// </summary>
		protected string contentIdPostfix;

		/// <summary>
		/// Message text formatter used to format nntp messages.
		/// </summary>
		/// <param name="contentIdPostfix">Generated images' content-id postfix.</param>
		/// <param name="maxSize">Maximum size of all attached images, 0 - not limit</param>
		/// <param name="proxy">Proxy to retrieve extrenal resources.</param>
  	public ImageProcessor(string contentIdPostfix, long maxSize, IWebProxy proxy)
  	{
			this.contentIdPostfix = contentIdPostfix;
			this.maxSize = maxSize;
			this.proxy = proxy;
			ProcessImagesDelegate = ProcessImages;
  	}

  	readonly NameValueCollection processedImagesIDs = new NameValueCollection();
  	readonly List<Message> processedImages = new List<Message>();
		private long processedImagesSize;
		private readonly long maxSize;

		/// <summary>
		/// Array of processed during message formatting inline images.
		/// </summary>
		/// <returns></returns>
  	public Message[] GetProcessedImages()
  	{
			return processedImages.ToArray();
  	}
		/// <summary>
		/// Number of processed inline images.
		/// </summary>
		public int ProcessedImagesCount
		{
			get { return processedImages.Count; }
		}

		/// <summary>
		/// Clear all processed images.
		/// </summary>
		public void ClearProcessedImages()
		{
			processedImagesIDs.Clear();
			processedImages.Clear();
			processedImagesSize = 0;
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
				var imgContentID = processedImagesIDs[image.Groups["url"].Value];
				if (imgContentID == null)
				{
					var req = WebRequest.Create(image.Groups["url"].Value);
					req.Proxy = proxy;
					response = req.GetResponse();
					if ((maxSize == 0) ||
							(response.ContentLength + processedImagesSize <= maxSize))
					{
						var imgPart = new Message(false) {ContentType = response.ContentType};
						var idGuid = Guid.NewGuid();
						imgPart["Content-ID"] = string.Format("<{0}{1}>", idGuid, contentIdPostfix);
						imgPart["Content-Location"] = imgContentID =
							Format.EncodeAgainstXSS(image.Groups["url"].Value);
						imgPart["Content-Disposition"] = "inline";
						imgPart.TransferEncoding = ContentTransferEncoding.Base64;
						using (var reader = new BinaryReader(response.GetResponseStream()))
						{
							imgPart.Entities.Add(reader.ReadBytes((int)response.ContentLength));
						}
						processedImages.Add(imgPart);
						processedImagesIDs[image.Groups["url"].Value] = imgContentID;
						processedImagesSize += response.ContentLength;
					}
					else
						return formatter.ProcessImages(image);
				}
				return string.Format("<img border='0' src='{0}' />", imgContentID);
			}
			catch (Exception ex)
			{
				logger.Warn(string.Format("Image {0} not found.", image.Groups["url"].Value), ex);
				return formatter.ProcessImages(image);
			}
			finally
			{
				if (response != null)
					response.Close();
			}
  	}

		/// <summary>
		/// Delegate to process images.
		/// </summary>
		public TextFormatter.ProcessImagesDelegate ProcessImagesDelegate;

	}
}