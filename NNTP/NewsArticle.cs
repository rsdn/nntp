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
		public NewsArticle(string messageID, string[] newsGroups, int[] messageNumbers)
		{
			if (newsGroups.Length != messageNumbers.Length)
				throw new ArgumentException("Size of newsGroups and messageNumbers parameters must be the same.");

			StringBuilder newsGroupsHeader = new StringBuilder();
			MessageID = messageID;

			this.messageNumbers = CollectionsUtil.CreateCaseInsensitiveHashtable(newsGroups.Length);
			for (int i = 0; i < newsGroups.Length; i++)
			{
				newsGroupsHeader.Append(newsGroups[i]).Append(" ");
				this.messageNumbers[newsGroups[i]] = messageNumbers[i];
			}
			
			this["Newsgroups"] = newsGroupsHeader.ToString();
		}

		protected	Hashtable messageNumbers;
		public Hashtable MessageNumbers
		{
			get { return messageNumbers; }
		}

		public enum Content {None, Header, Body, HeaderAndBody }

		public string MessageID
		{
			get { return this["Message-ID"]; }
			set { this["Message-ID"] = value; }
		}
		/// <summary>
		/// type of content of article
		/// </summary>
		protected Content content = Content.None;
	}
}