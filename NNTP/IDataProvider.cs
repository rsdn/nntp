using System;
using derIgel.MIME;

namespace derIgel.NNTP
{
	/// <summary>
	/// data provider interface
	/// </summary>
	public interface IDataProvider
	{
		NewsArticle GetArticle(string originalMessageID,
			NewsArticle.Content content);

		NewsArticle GetArticle(int articleNumber,
			NewsArticle.Content content);

		NewsArticle[] GetArticleList(int startNumber, int endNumber,
			NewsArticle.Content content);

		NewsArticle GetArticle(NewsArticle.Content content);

		NewsArticle GetNextArticle();

		NewsArticle GetPrevArticle();

		NewsArticle[] GetArticleList(string[] newsgroups, DateTime date, string[] distributions);
		
		NewsGroup GetGroup(string groupName);

		NewsGroup[] GetGroupList(DateTime startDate, string[] distributions);

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
		/// true, if posting allowed for this provider
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

		void Config(NNTPSettings settings);
	}
}