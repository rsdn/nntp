using System;
using derIgel.MIME;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// abstract data provider
		/// </summary>
		public abstract class DataProvider
		{

			public enum Errors 
			{
				UnknownError, NoSelectedGroup, NoSelectedArticle, NoSuchArticleNumber,
				NoSuchArticle, NoNextArticle, NoPrevArticle, NoSuchGroup, NoPermission,
				NotSupported, PostingFailed, ServiceUnaviable
			} 

			public class Exception : ApplicationException
			{
				public Exception(Errors error) :
					base("DataProvider error")
				{
					this.error = error;
				}

				public Exception(Errors error, System.Exception innerException) :
					base("DataProvider error", innerException)
				{
					this.error = error;
				}

				protected Errors error;

				public Errors Error
				{
					get
					{
						return error;
					}
				}
			};

			protected string currentGroup = null;
			protected int currentArticle = -1;

			public DataProvider(object settings)
			{

			}

			public abstract NewsArticle GetArticle(string originalMessageID,
				NewsArticle.Content content);

			public abstract NewsArticle GetArticle(int articleNumber,
				NewsArticle.Content content);

			public abstract NewsArticle[] GetArticleList(int startNumber, int endNumber,
				NewsArticle.Content content);

			public virtual NewsArticle GetArticle(NewsArticle.Content content)
			{
				if (currentArticle == -1)
					throw new DataProvider.Exception((currentGroup == null) ?
						Errors.NoSelectedGroup : Errors.NoSelectedArticle);
	
				return GetArticle(currentArticle, content);
			}

			public abstract NewsArticle GetNextArticle();

			public abstract NewsArticle GetPrevArticle();

			public abstract NewsArticle[] GetArticleList(string[] newsgroups, DateTime date, string[] distributions);
			
			public abstract NewsGroup GetGroup(string groupName);

			public abstract NewsGroup[] GetGroupList(DateTime startDate, string[] distributions);

			public virtual bool Authentificate(string user, string pass)
			{
				return true;
			}

			public abstract void PostMessage(Message article);

			protected internal string username = "";
			protected internal string password = "";
			
			
			protected bool postingAllowed = false;
			/// <summary>
			/// true, if posting allowed for this provider
			/// </summary>
			public bool PostingAllowed
			{
				get {return postingAllowed; }
			}

			protected Session.States initialSessionState = Session.States.Normal;
			/// <summary>
			/// Initial session's state
			/// </summary>
			public Session.States InitialSessionState
			{
				get {return initialSessionState;}
			}
		}
	}
}