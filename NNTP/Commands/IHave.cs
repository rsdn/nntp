using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// IHAVE command.
	/// </summary>
	[NntpCommand("IHAVE")]
	public class IHave : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex PostSyntaxisChecker =
			new Regex(@"(?in)^IHAVE[ \t]+(?<messageID><\S+>)[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public IHave(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = PostSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			if (session.DataProvider.WantArticle(lastMatch.Groups["messageID"].Value))
			{
				// Transfer artcticle is waiting
				session.sessionState = Session.States.TransferWaiting;
				return new Response(NntpResponse.TransferArticle);
			}
			return new Response(NntpResponse.ArticleNotWanted);
		}
	}
}
