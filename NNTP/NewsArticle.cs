using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using derIgel.MIME;

namespace derIgel.NNTP
{
	/// <summary>
	/// Newsgroup article
	/// </summary>
	[Serializable]
	public class NewsArticle : Message
	{
		public NewsArticle(string messageID, int messageNumber)
		{
			this.messageNumber = messageNumber;
			this["Message-ID"] = messageID;
		}

		protected	int messageNumber;

		public enum Content {None, Header, Body, HeaderAndBody }

		public string MessageID
		{
			get { return this["Message-ID"]; }
			set { this["Message-ID"] = value; }
		}
		public int Number
		{
			get	{	return messageNumber;	}
			set	{	messageNumber = value;}
		}

		/// <summary>
		/// type of content of article
		/// </summary>
		protected Content content = Content.None;
	}
}