using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// LAST command.
	/// </summary>
	[NntpCommand("LAST")]
	public class Last : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex LastSyntaxisChecker =
			new Regex(@"(?in)^LAST[ \t]*$", RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Last(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = LastSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response.</returns>
		protected override Response ProcessCommand()
		{
			if (session.currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			if (session.currentArticle == -1)
				throw new DataProviderException(DataProviderErrors.NoSelectedArticle);

			var article = session.DataProvider.GetPrevArticle(session.currentArticle, session.currentGroup);

			session.currentArticle = article.MessageNumbers[session.currentGroup];

			return new Response(NntpResponse.ArticleNothingRetrivied, null,
				article.MessageNumbers[session.currentGroup], article["Message-ID"]);
		}
	}
}
