using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// SLAVE command.
	/// </summary>
	[NntpCommand("SLAVE")]
	public class Slave : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex SlaveSyntaxisChecker =
			new	Regex(@"(?in)^SLAVE[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Slave(Session session) : base(session)
		{
			syntaxisChecker = SlaveSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response.</returns>
		protected override Response ProcessCommand()
		{
			// Taken into account (really not :) ).
			return new Response(NntpResponse.Slave);
		}
	}
}
