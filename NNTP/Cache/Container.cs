using System;

using Rsdn.Nntp;

namespace Rsdn.Nntp.Cache
{
	/// <summary>
	/// Summary description for Container.
	/// </summary>
	public class Container
	{
		public Container(NewsArticle article, NewsArticle.Content content)
		{
			Article = article;
			Content = content;
		}

		public NewsArticle Article;
		public NewsArticle.Content Content;
	}
}
