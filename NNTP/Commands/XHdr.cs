using System;
using System.Text;
using System.Text.RegularExpressions;

using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// XHDR command realization
	/// </summary>
	[NntpCommand("XHDR")]
	public class XHdr : Generic
	{
		/// <summary>
		/// Create XHDR command class.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public XHdr(Session session) : base(session)	
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = xhdrSyntaxisChecker;
		}

		/// <summary>
		/// Syntaxis checker for XHDR command.
		/// </summary>
		protected static Regex xhdrSyntaxisChecker =
			new Regex(@"(?in)^XHDR[ \t]+(?<header>\S+)" + 
								@"([ \t]+((?<messageID><\S+>)|(?<startNumber>\d+)([ \t]*(?<dash>-)[ \t]*(?<endNumber>\d+)?)?))?[ \t]*$",
								RegexOptions.Compiled);

		/// <summary>
		/// Process XHDR command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			NewsArticle[] articleList;
			if (lastMatch.Groups["messageID"].Success)
			{
				articleList = new NewsArticle[1];
				articleList[0] = session.DataProvider.GetArticle(lastMatch.Groups["messageID"].Value, NewsArticle.Content.Header);
			}
			else if (lastMatch.Groups["startNumber"].Success)
			{
				if (session.currentGroup == null)
					throw new DataProviderException(DataProviderErrors.NoSelectedGroup);

				int startNumber = Convert.ToInt32(lastMatch.Groups["startNumber"].Value);
				int endNumber = startNumber;
				if (lastMatch.Groups["dash"].Success)
					if (lastMatch.Groups["endNumber"].Success)
						endNumber = Convert.ToInt32(lastMatch.Groups["endNumber"].Value);
					else
						endNumber = -1;
				articleList = session.DataProvider.
					GetArticleList(startNumber, endNumber, session.currentGroup, NewsArticle.Content.Header);
			}
			else
			{
				if (session.currentArticle == -1)
					throw new DataProviderException((session.currentGroup == null) ?
						DataProviderErrors.NoSelectedGroup : DataProviderErrors.NoSelectedArticle);

				articleList = new NewsArticle[1];
				articleList[0] = session.DataProvider.GetArticle(session.currentArticle, session.currentGroup, NewsArticle.Content.Header);
			}

			if (articleList.Length > 0)
			{
				string header = lastMatch.Groups["header"].Value;
				StringBuilder output = new StringBuilder();
				foreach (NewsArticle article in articleList)
					if (article[header] != null)
					{
						output.Append(lastMatch.Groups["messageID"].Success ? article.MessageID : article.MessageNumbers[session.currentGroup].ToString());
						output.Append(' ').Append(Header.Unfold(article.EncodedHeader(header))).Append(Util.CRLF);
					}
				return new Response(NntpResponse.ArticleHeadRetrivied, output.ToString(), null, null);
			}
			else
				return new Response(NntpResponse.NoSelectedArticle);
		}
	}
}
