using System;

namespace Rsdn.Nntp
{
	/// <summary>
	/// News Group
	/// </summary>
	public class NewsGroup
	{
		public NewsGroup(string name, int firstArticleNumber, int lastArticleNumber,
			int etimatedArticles, bool postingAllowed, DateTime created)
		{
			this.name = name;
			this.firstArticleNumber = firstArticleNumber;
			this.lastArticleNumber = lastArticleNumber;
			this.etimatedArticles = etimatedArticles;
			this.postingAllowed = postingAllowed;
			this.created = created;
		}
		protected	string name;
		protected string description;
		protected int firstArticleNumber;
		protected int lastArticleNumber;
		protected int etimatedArticles;
		protected bool postingAllowed;
		protected DateTime created;

		public string Name
		{
			get
			{
				return name;
			}
		}

		public string Description
		{
			get {return description;}
			set {description = value;}
		}

		public int FirstArticleNumber
		{
			get
			{
				return firstArticleNumber;
			}
		}

		public int LastArticleNumber
		{
			get
			{
				return lastArticleNumber;
			}
		}

		public int EtimatedArticles
		{
			get
			{
				return etimatedArticles;
			}
		}

		public bool PostingAllowed
		{
			get
			{
				return postingAllowed;
			}
		}

		public DateTime Created
		{
			get
			{
				return created;
			}
		}
	}
}