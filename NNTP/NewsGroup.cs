using System;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// News Group
		/// </summary>
		public class NewsGroup
		{
			public NewsGroup(string name, int firstArticleNumber, int lastArticleNumber,
				int etimatedArticles, bool postingAllowed)
			{
				this.name = name;
				this.firstArticleNumber = firstArticleNumber;
				this.lastArticleNumber = lastArticleNumber;
				this.etimatedArticles = etimatedArticles;
				this.postingAllowed = postingAllowed;
			}
			protected	string name;
			protected string description;
			protected int firstArticleNumber;
			protected int lastArticleNumber;
			protected int etimatedArticles;
			protected bool postingAllowed;

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
		}
	}
}