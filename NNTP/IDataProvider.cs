using System;

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
		/// Retrive article.
		/// </summary>
		/// <param name="articleNumber">Article's number.</param>
		/// <param name="content">Necessary content of article.</param>
		/// <returns>News article.</returns>
		NewsArticle GetArticle(int articleNumber,
			NewsArticle.Content content);

		/// <summary>
		/// retrive article list.
		/// </summary>
		/// <param name="startNumber">Start article's number.</param>
		/// <param name="endNumber">End article's number.</param>
		/// <param name="content">Necessary content of articles.</param>
		/// <returns>List of news articles.</returns>
		NewsArticle[] GetArticleList(int startNumber, int endNumber,
			NewsArticle.Content content);

		/// <summary>
		/// Get selected article.
		/// </summary>
		/// <param name="content">Necessary content of article.</param>
		/// <returns>News article.</returns>
		NewsArticle GetArticle(NewsArticle.Content content);

		/// <summary>
		/// Get next article.
		/// </summary>
		/// <returns>News article.</returns>
		NewsArticle GetNextArticle();

		/// <summary>
		/// Get previous article.
		/// </summary>
		/// <returns>News article.</returns>
		NewsArticle GetPrevArticle();

		/// <summary>
		/// Get article list.
		/// </summary>
		/// <param name="newsgroups">Newsgroups' names pattern.</param>
		/// <param name="date">Start date.</param>
		/// <param name="distributions">Distribution parameter.</param>
		/// <returns>List of news articles.</returns>
		NewsArticle[] GetArticleList(string[] newsgroups, DateTime date, string[] distributions);
		
		/// <summary>
		/// Get news group's description.
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns></returns>
		NewsGroup GetGroup(string groupName);

		/// <summary>
		/// Get list of news groups' description.
		/// </summary>
		/// <param name="startDate">Start date.</param>
		/// <param name="distributions">Distribution parameter.</param>
		/// <returns></returns>
		NewsGroup[] GetGroupList(DateTime startDate, string[] distributions);

		/// <summary>
		/// Authentificate user.
		/// </summary>
		/// <param name="user">User name.</param>
		/// <param name="pass">User password.</param>
		/// <returns>True if authentificated.</returns>
		bool Authentificate(string user, string pass);

		void PostMessage(Message article);

		/// <summary>
		/// Get current selected group.
		/// 'null' if no group selected
		/// </summary>
		string CurrentGroup
		{
			get;
		}

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
	}
}