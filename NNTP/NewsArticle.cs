using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Rsdn.Mime;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Newsgroup article
	/// </summary>
	[Serializable]
	public class NewsArticle : Message
	{
		public NewsArticle(string messageID, string[] newsGroups, int[] messageNumbers, Content content)
		{
			if (newsGroups.Length != messageNumbers.Length)
				throw new ArgumentException("Size of newsGroups and messageNumbers parameters must be the same.");

			StringBuilder newsGroupsHeader = new StringBuilder();
			MessageID = messageID;

			_messageNumbers = new Dictionary<string, int>(newsGroups.Length);
			for (int i = 0; i < newsGroups.Length; i++)
			{
				newsGroupsHeader.Append(newsGroups[i]).Append(" ");
				_messageNumbers[newsGroups[i]] = messageNumbers[i];
			}
			// remove last space, if exists
			if (newsGroupsHeader.Length > 0)
				newsGroupsHeader.Length--;
			
			this["Newsgroups"] = newsGroupsHeader.ToString();
			Contents = content;
		}

		protected	IDictionary<string, int> _messageNumbers;
    public IDictionary<string, int> MessageNumbers
		{
			get { return _messageNumbers; }
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
		public Content Contents = Content.None;
	}
}