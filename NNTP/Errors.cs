using System;

namespace Rsdn.Nntp
{
	public enum DataProviderErrors
	{
		UnknownError, NoSelectedGroup, NoSelectedArticle, NoSuchArticleNumber,
		NoSuchArticle, NoNextArticle, NoPrevArticle, NoSuchGroup, NoPermission,
		NotSupported, PostingFailed, ServiceUnaviable
	} 

	public class DataProviderException  : ApplicationException
	{
		public DataProviderException (DataProviderErrors error) :
			base("DataProvider error")
		{
			this.error = error;
		}

		public DataProviderException (DataProviderErrors error, Exception innerException) :
			base(string.Format("DataProvider error ({0})", error), innerException)
		{
			this.error = error;
		}

		protected DataProviderErrors error;

		public DataProviderErrors Error
		{
			get
			{
				return error;
			}
		}
	}
}