using System;
using Rsdn.Framework.Formatting;
using Rsdn.RsdnNntp.Public.RsdnService;

namespace Rsdn.RsdnNntp.Public
{
	/// <summary>
	/// Summary description for Article.
	/// </summary>
	public class Article : IArticle
	{
		protected article message;

		public Article(article message)
		{
			this.message = message;
		}

		public int ID
		{
			get {  return Format.ToInt(message.id); }
		}
		public int ParentID
		{
			get { return Format.ToInt(message.pid); }
		}
		public string Postfix
		{
			get { return message.postfix; }
		}
		public int Number
		{
			get { return message.num; }
		}
		public string Author
		{
			get { return message.author; }
		}
		public int AuthorID
		{
			get { return Format.ToInt(message.authorid); }
		}
		public string Subject
		{
			get { return message.subject; }
		}
		public DateTime Date
		{
			get { return message.date; }
		}
		public string Message
		{
			get { return message.message; }
		}
		public string HomePage
		{
			get { return message.homePage; }
		}
		public bool Smile
		{
			get { return message.smile; }
		}
		public string UserType
		{
			get { return message.userType; }
		}
		public int UserColor
		{
			get { return message.userColor; }
		}
		public string Group
		{
			get { return message.group; }
		}
		public int GroupID
		{
			get { return Format.ToInt(message.gid); }
		}
	}
}
