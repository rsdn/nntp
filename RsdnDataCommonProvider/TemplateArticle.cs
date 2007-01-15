using System;
using System.Collections.Generic;
using System.Text;

namespace Rsdn.RsdnNntp
{
	internal class TemplateArticle : IArticle
	{
		IArticle _article;
		public TemplateArticle(IArticle article)
		{
			_article = article;
		}

		public string UserColorWeb
		{
			get { return string.Format("#{0:x6}", _article.UserColor); }
		}

		public int ID
		{
			get { return _article.ID; }
		}

		public int ParentID
		{
			get { return _article.ParentID; }
		}

		public string Postfix
		{
			get { return _article.Postfix; }
		}

		public int Number
		{
			get { return _article.Number; }
		}

		public string Author
		{
			get { return _article.Author; }
		}

		public int AuthorID
		{
			get { return _article.AuthorID; }
		}

		public string Subject
		{
			get { return _article.Subject; }
		}

		public DateTime Date
		{
			get { return _article.Date; }
		}

		public string Message
		{
			get { return _article.Message; }
		}

		public string HomePage
		{
			get { return _article.HomePage; }
		}

		public bool Smile
		{
			get { return _article.Smile; }
		}

		public string UserType
		{
			get { return _article.UserType; }
		}

		public int UserColor
		{
			get { return _article.UserColor; }
		}

		public string Group
		{
			get { return _article.Group; }
		}

		public int GroupID
		{
			get { return _article.GroupID; }
		}
	}
}
