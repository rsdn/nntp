using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// POST command.
	/// </summary>
	[NntpCommand("POST")]
	public class Post : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex PostSyntaxisChecker =
			new Regex(@"(?in)^POST[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Post(Session session) : base(session)
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
			session.sessionState = Session.States.PostWaiting;
			return new Response(NntpResponse.SendArticle);
		}
	}
}
