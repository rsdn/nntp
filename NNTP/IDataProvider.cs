using System;
using System.Net;

using Rsdn.Mime;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Data provider's interface
	/// </summary>
	public interface IDataProvider : IDisposable
	{
		/// <summary>
		/// Data Provider's identification string (usually short name plus version)
		/// </summary>
		string Identity
		{
			get;
		}

		Type GetConfigType();

		NewsArticle GetArticle(string originalMessageID,
			NewsArticle.Content content);

		/// <summary>
		/// Retrive article by number.
		/// </summary>
		/// <param name="articleNumber">Article's number.</param>
		/// <param name="groupName">Article's group name.</param>
		/// <param name="content">Necessary content of article.</param>
		/// <returns>News article.</returns>
		NewsArticle GetArticle(int articleNumber, string groupName,
			NewsArticle.Content content);

		/// <summary>
		/// Retrive article list.
		/// </summary>
		/// <param name="startNumber">Start article's number.</param>
		/// <param name="endNumber">End article's number.</param>
		/// <param name="groupName">News group.</param>
		/// <param name="content">Necessary content of articles.</param>
		/// <returns>List of news articles.</returns>
		NewsArticle[] GetArticleList(int startNumber, int endNumber,
			string groupName, NewsArticle.Content content);

		/// <summary>
		/// Get next article IDs.
		/// Only article number & MessageID required.
		/// </summary>
		/// <param name="messageNumber">Current message number.</param>
		/// <param name="groupName">Current news group.</param>
		/// <returns>Next article's IDs.</returns>
		NewsArticle GetNextArticle(int messageNumber, string groupName);

		/// <summary>
		/// Get previous article IDs.
		/// Only article number & MessageID required.
		/// </summary>
		/// <param name="messageNumber">Current message number.</param>
		/// <param name="groupName">Current news group.</param>
		/// <returns>Previous article's IDs.</returns>
		NewsArticle GetPrevArticle(int messageNumber, string groupName);

		/// <summary>
		/// Get article list.
		/// </summary>
		/// <param name="date">Start date.</param>
		/// <param name="pattern">Group name pattern.</param>
		/// <returns>List of articles.</returns>
		NewsArticle[] GetArticleList(DateTime date, string pattern);
		
		/// <summary>
		/// Get news group's description.
		/// </summary>
		/// <param name="groupName">News group name.</param>
		/// <returns>News group description.</returns>
		NewsGroup GetGroup(string groupName);

		/// <summary>
		/// Get list of news groups' descriptions.
		/// </summary>
		/// <param name="startDate">Start date.</param>
		/// <param name="pattern">News group's name match pattern.</param>
		/// <returns>List of news groups' descriptions.</returns>
		NewsGroup[] GetGroupList(DateTime startDate, string pattern);

		/// <summary>
		/// Authentificate user.
		/// </summary>
		/// <param name="user">User name.</param>
		/// <param name="pass">User password.</param>
		/// <param name="ip">User's connection ip address.</param>
		/// <returns>True if authentificated.</returns>
		bool Authentificate(string user, string pass, IPAddress ip);

		/// <summary>
		/// Post message.
		/// </summary>
		/// <param name="article"></param>
		void PostMessage(Message article);

		/// <summary>
		/// True, if posting allowed for this provider
		/// </summary>
		bool PostingAllowed
		{
			get;
		}
		/// <summary>
		/// Initial session's state
		/// </summary>
		Session.States InitialSessionState
		{
			get;
		}

		/// <summary>
		/// Configure data provider.
		/// </summary>
		/// <param name="settings"></param>
		void Config(object settings);

		/// <summary>
		/// Check if data provider want to get article.
		/// </summary>
		/// <param name="messageID"></param>
		/// <returns></returns>
		bool WantArticle(string messageID);
	}
}