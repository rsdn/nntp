using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// DATE command.
	/// </summary>
	[NntpCommand("DATE")]
	public class Date : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex QuitSyntaxisChecker =
			new Regex(@"(?in)^DATE[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Date(Session session) : base(session)
		{
			syntaxisChecker = QuitSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Date, null, DateTime.UtcNow.ToString("yyyyMMddhhmmss"));
		}
	}
}
