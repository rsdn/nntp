using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// MODE READER and MODE STREAM commands.
	/// </summary>
	[NntpCommand("MODE")]
	public class Mode : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex ModeSyntaxisChecker =
			new Regex(@"(?in)^MODE[ \t]+(?<mode>READER|STREAM)[ \t]*$",	RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Mode(Session session) : base(session)
		{
			syntaxisChecker = ModeSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response.</returns>
		protected override Response ProcessCommand()
		{
			Response result;
			if ("READER".Equals(lastMatch.Groups["mode"].Value, StringComparison.OrdinalIgnoreCase))
				// MODE READER
				result = new Response(session.DataProvider.PostingAllowed ?
					NntpResponse.Ok : NntpResponse.OkNoPosting, null,
					string.Format("{0} ({1}; {2})",
						session.Manager.Name, Manager.ServerID, session.DataProvider.Identity));
			else
				// MODE STREAM
				result = new Response(NntpResponse.NotRecognized);
			return result;
		}
	}
}
