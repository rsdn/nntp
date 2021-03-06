using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// NEXT command.
	/// </summary>
	[NntpCommand("NEXT")]
	public class Next : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex NextSyntaxisChecker =
			new Regex(@"(?in)^NEXT[ \t]*$",	RegexOptions.Compiled);

		/// <summary>
		/// Create NEXT command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Next(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NextSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			if (session.currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			if (session.currentArticle == -1)
				throw new DataProviderException(DataProviderErrors.NoSelectedArticle);

			var article = session.DataProvider.GetNextArticle(session.currentArticle, session.currentGroup);

			session.currentArticle = article.MessageNumbers[session.currentGroup];

			return new Response(NntpResponse.ArticleNothingRetrivied, null,
				article.MessageNumbers[session.currentGroup], article["Message-ID"]);
		}
	}
}
