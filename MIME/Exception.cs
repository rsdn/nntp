using System;

namespace Rsdn.Mime
{
	/// <summary>
	/// Exception for MIME message parsing errors.
	/// </summary>
	public class MimeFormattingException : ApplicationException
	{
		/// <summary>
		/// Construct exception object.
		/// </summary>
		/// <param name="text">Error description.</param>
		public MimeFormattingException(string text) : base(text) {}
	}
}
