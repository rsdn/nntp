using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// QUIT command.
	/// </summary>
	[NntpCommand("QUIT")]
	public class Quit : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex QuitSyntaxisChecker =
			new Regex(@"(?in)^QUIT[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Quit(Session session) : base(session)
		{
			syntaxisChecker = QuitSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Bye);
		}
	}
}
