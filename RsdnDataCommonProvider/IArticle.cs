using System;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Generic (common) article interface.
	/// </summary>
	public interface IArticle
	{
		/// <summary>
		/// Message ID.
		/// </summary>
		int ID
		{
			get;
		}

		/// <summary>
		/// Message's parent ID.
		/// </summary>
		int ParentID
		{
			get;
		}
		
		/// <summary>
		/// Server postfix for generate message's IDs.
		/// </summary>
		string Postfix
		{
			get;
		}

		/// <summary>
		/// Message number in group.
		/// </summary>
		int Number
		{
			get;
		}

		/// <summary>
		/// Author.
		/// </summary>
		string Author
		{
			get;
		}

		/// <summary>
		/// Author ID.
		/// </summary>
		int AuthorID
		{
			get;
		}

		/// <summary>
		/// Message's subject.
		/// </summary>
		string Subject
		{
			get;
		}

		/// <summary>
		/// Message's date.
		/// </summary>
		DateTime Date
		{
			get;
		}
		
		/// <summary>
		/// Message's text.
		/// </summary>
		string Message
		{
			get;
		}

		/// <summary>
		/// Author's home page.
		/// </summary>
		string HomePage
		{
			get;
		}

		/// <summary>
		/// Format or not smiles in message.
		/// </summary>
		bool Smile
		{
			get;
		}

		/// <summary>
		/// Author's user type.
		/// </summary>
		string UserType
		{
			get;
		}

		/// <summary>
		/// Author's user color.
		/// </summary>
		int UserColor
		{
			get;
		}

		/// <summary>
		/// Message's group name.
		/// </summary>
		string Group
		{
			get;
		}

		/// <summary>
		/// Message's group id.
		/// </summary>
		int GroupID
		{
			get;
		}
		
	}
}