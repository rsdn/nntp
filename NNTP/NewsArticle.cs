using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using derIgel.Utils;

namespace derIgel.NNTP
{
	/// <summary>
	/// Newsgroup article
	/// </summary>
	[Serializable]
	public class NewsArticle : derIgel.Mail.Message
	{
		public NewsArticle(string messageID, int messageNumber)
		{
			this.messageNumber = messageNumber;
			BodyEncoding = BodyEncodingEnum.Base64;
			systemHeaders["Message-ID"] = messageID;
		}

		protected	int messageNumber;

		public enum Content {None, Header, Body, HeaderAndBody }

		public string MessageID
		{
			get { return this["Message-ID"]; }
		}
		public int Number
		{
			get	{	return messageNumber;	}
		}

		/// <summary>
		/// type of content of article
		/// </summary>
		protected Content content = Content.None;
	}
}