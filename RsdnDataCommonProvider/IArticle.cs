using System;

namespace Rsdn.RsdnNntp
{
	public interface IArticle
	{
		int ID
		{
			get;
		}

		int ParentID
		{
			get;
		}
		
		string Postfix
		{
			get;
		}

		int Number
		{
			get;
		}

		string Author
		{
			get;
		}

		int AuthorID
		{
			get;
		}

		string Subject
		{
			get;
		}

		DateTime Date
		{
			get;
		}
		
		string Message
		{
			get;
		}

		string HomePage
		{
			get;
		}

		bool Smile
		{
			get;
		}

		string UserType
		{
			get;
		}

		int UserColor
		{
			get;
		}

		string Group
		{
			get;
		}

		int GroupID
		{
			get;
		}
		
	}
}