using System;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Possible Data Provider's errors.
	/// </summary>
	public enum DataProviderErrors
	{
		/// <summary>
		/// Generic, unknown error.
		/// </summary>
		UnknownError,
		/// <summary>
		/// No one group is selected.
		/// </summary>
		NoSelectedGroup,
		/// <summary>
		/// No one article is selected.
		/// </summary>
		NoSelectedArticle,
		/// <summary>
		/// No such article number.
		/// </summary>
		NoSuchArticleNumber,
		/// <summary>
		/// No such article (retriving by messageID)
		/// </summary>
		NoSuchArticle,
		/// <summary>
		/// There is no next article.
		/// </summary>
		NoNextArticle,
		/// <summary>
		/// There is no previous article.
		/// </summary>
		NoPrevArticle,
		/// <summary>
		/// There is no such group.
		/// </summary>
		NoSuchGroup,
		/// <summary>
		/// Don't have permission to do this.
		/// </summary>
		NoPermission,
		/// <summary>
		/// Command is not supported.
		/// </summary>
		NotSupported,
		/// <summary>
		/// Article posting failed.
		/// </summary>
		PostingFailed,
		/// <summary>
		/// Service is unaviable.
		/// </summary>
		ServiceUnaviable
	} 

	/// <summary>
	/// Data Provider's specific exception.
	/// </summary>
	public class DataProviderException  : ApplicationException
	{
		/// <summary>
		/// Create Data Provider exception with specific error.
		/// </summary>
		/// <param name="error">Specific Data Provider's error.</param>
		public DataProviderException (DataProviderErrors error) :
			base("DataProvider error")
		{
			this.error = error;
		}

		/// <summary>
		/// Create Data Provider exception with unknown error and error description.
		/// </summary>
		/// <param name="errorDescription">Error description.</param>
		public DataProviderException (string errorDescription) :
			base(string.Format("DataProvider error ({0})", errorDescription))
		{
			error = DataProviderErrors.UnknownError;
		}

		/// <summary>
		/// Create Data Provider exception with specific error and inner exception.
		/// </summary>
		/// <param name="error">Specific Data Provider's error.</param>
		/// <param name="innerException">Inner exception.</param>
		public DataProviderException (DataProviderErrors error, Exception innerException) :
			base(string.Format("DataProvider error ({0})", error), innerException)
		{
			this.error = error;
		}

		/// <summary>
		/// Internal storage for data provider's error.
		/// </summary>
		protected DataProviderErrors error;

		/// <summary>
		/// Data provider's error.
		/// </summary>
		public DataProviderErrors Error
		{
			get
			{
				return error;
			}
		}
	}
}