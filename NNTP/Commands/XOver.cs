using System;
using System.Text;
using System.Text.RegularExpressions;

using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary command.
	/// </summary>
	[NntpCommand("XOVER")]
	public class Xover : Generic
	{
		/// <summary>
		/// Syntaxis checker for XOVER command.
		/// </summary>
		protected static Regex XoverSyntaxisChecker =
			new Regex(@"(?in)^XOVER([ \t]+(?<startNumber>\d+)" + 
			@"([ \t]*(?<dash>-)[ \t]*(?<endNumber>\d+)?)?)?[ \t]*$",
			RegexOptions.Compiled);

		/// <summary>
		/// Create XOVER command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Xover(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = XoverSyntaxisChecker;
		}

		/// <summary>
		/// Process XOVER command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			if (session.currentGroup == null)
				throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

			NewsArticle[] articleList;
			if (lastMatch.Groups["startNumber"].Success)
			{
				int startNumber = Convert.ToInt32(lastMatch.Groups["startNumber"].Value);
				int endNumber = startNumber;
				if (lastMatch.Groups["dash"].Success)
					if (lastMatch.Groups["endNumber"].Success)
						endNumber = Convert.ToInt32(lastMatch.Groups["endNumber"].Value);
					else
						endNumber = int.MaxValue;
				articleList = session.DataProvider.
					GetArticleList(startNumber, endNumber, session.currentGroup, NewsArticle.Content.Header);
			}
			else
			{
				if (session.currentArticle == -1)
					throw new DataProviderException(DataProviderErrors.NoSelectedArticle);

				articleList = new NewsArticle[1];
				articleList[0] = session.DataProvider.GetArticle(session.currentArticle, session.currentGroup, NewsArticle.Content.Header);
			}

			if (articleList.Length > 0)
			{
				StringBuilder output = new StringBuilder();
				foreach (NewsArticle article in articleList)
				{
					output.Append(ModifyArticle(article).MessageNumbers[session.currentGroup]);
					foreach (string headerItem in List.headerItems)
						// replace in *unfolded* header all non-good symbols to space
						output.Append('\t').
							Append(article[headerItem] == null ? null : 
								Regex.Replace(Header.Unfold(article.EncodedHeader(headerItem)), @"\s", " ")).
							Append(Util.CRLF);
				}
				return new Response(NntpResponse.Overview, output.ToString());
			}
			else
				return new Response(NntpResponse.NoSelectedArticle);
		}
	}
}
