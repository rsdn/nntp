using System;

namespace derIgel.MIME
{
	/// <summary>
	/// Summary description for Exception.
	/// </summary>
	public class MimeFormattingException : ApplicationException
	{
		public MimeFormattingException(string text) : base(text) {}
	}
}