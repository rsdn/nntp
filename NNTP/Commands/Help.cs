using System.Text;
using System.Text.RegularExpressions;
using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// HELP command.
	/// </summary>
	[NntpCommand("HELP")]
	public class Help : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex HelpSyntaxisChecker =
			new Regex(@"(?in)^HELP[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Help(Session session) : base(session)
		{
			syntaxisChecker = HelpSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			var supportCommands = new StringBuilder();
			supportCommands.Append(Manager.ServerID).Append(" ").
				Append("supports follow commands:").Append(Util.CRLF);
			foreach (var command in session.commands.Keys)
				supportCommands.AppendFormat("\t{0}{1}", command,Util.CRLF);
			return new Response(NntpResponse.Help, supportCommands.ToString());
		}
	}
}
