using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// Common class for ARTICLE, HEAD, BODY, STAT commands
	/// </summary>
	[NntpCommand("ARTICLE")]
	[NntpCommand("HEAD")]
	[NntpCommand("BODY")]
	[NntpCommand("STAT")]
	public class ArticleHeadBodyStat : Generic
	{
		/// <summary>
		/// Coomand syntaxis checker's regular expression
		/// </summary>
		protected static Regex ArticleHeadBodyStatSyntaxisChecker =
			new Regex(@"(?in)^(?<command>ARTICLE|HEAD|BODY|STAT)([ \t]+((?<messageID><\S+>)|(?<messageNumber>\d+)))?[ \t]*$",
			RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public ArticleHeadBodyStat(Session session) : base(session)	
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = ArticleHeadBodyStatSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			NntpResponse responseCode;
			NewsArticle.Content content;
			switch (lastMatch.Groups["command"].Value.ToUpper())
			{
				case "ARTICLE" :
					content = NewsArticle.Content.HeaderAndBody;
					responseCode = NntpResponse.ArticleHeadBodyRetrivied;
					break;
				case "HEAD"	:
					content = NewsArticle.Content.Header;
					responseCode = NntpResponse.ArticleHeadRetrivied;
					break;
				case "BODY"	:
					content = NewsArticle.Content.Body;
					responseCode = NntpResponse.ArticleBodyRetrivied;
					break;
				case "STAT"	:
					content = NewsArticle.Content.None;
					responseCode = NntpResponse.ArticleNothingRetrivied;
					break;
				default:
					content = NewsArticle.Content.None;
					responseCode = NntpResponse.ProgramFault;
					break;
			}

			NewsArticle article;
			if (lastMatch.Groups["messageID"].Success)
			{
				article = session.DataProvider.GetArticle(
					lastMatch.Groups["messageID"].Value, content);
			}
			else
				if (lastMatch.Groups["messageNumber"].Success)
				{
					article = session.DataProvider.GetArticle(
						Convert.ToInt32(lastMatch.Groups["messageNumber"].Value), content);
				}
				else
					article = session.DataProvider.GetArticle(content);
			
			ModifyArticle(article);
			return new Response(responseCode, article.GetBody(),
				// article retirived by messageID don't change "internal current pointer", so we may not have current group
				lastMatch.Groups["messageID"].Success ? null : article.MessageNumbers[session.DataProvider.CurrentGroup],
				article["Message-ID"] != null ? article["Message-ID"] : "<0>");
		}
	}
}
