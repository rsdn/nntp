using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// NEWNEWS command.
	/// </summary>
	[NntpCommand("NEWNEWS")]
	public class NewNews : Generic
	{
		protected static Regex NewNewsSyntaxisChecker =
			new	Regex(@"(?in)^NEWNEWS[ \t]+(?<newsgroups>!?(\*|\w+)(.(\*|\w+))*(,!?(\*|\w+)(.(\*|\w+))*)*)[ \t]+" + 
			@"(?<date>\d{6}[ \t]+\d{6})([ \t]+(?<timezone>GMT))?" + 
			@"([ \t]+<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
			RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public NewNews(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NewNewsSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response.</returns>
		protected override Response ProcessCommand()
		{
			try
			{
				var resultPattern = new StringBuilder("^");

				// get newsgroup names
				var positiveMatch = new StringBuilder();
				var negativeMatch = new StringBuilder();
				foreach (var group in lastMatch.Groups["newsgroups"].Value.Split(new[]{','}))
					if (group.StartsWith("!"))
						negativeMatch.AppendFormat("{0}|", TransformWildmat(group.Substring(1, group.Length - 1)));
					else
						positiveMatch.AppendFormat("{0}|", TransformWildmat(group));

				// get distributions if exist
				var distributions = new StringBuilder();
				if (lastMatch.Groups["distributions"].Success)
				{
					foreach (var pattern in lastMatch.Groups["distributions"].Value.Split(new[]{','}))
						distributions.AppendFormat(@"{0}\..*|", TransformWildmat(pattern));
					// remove last '|' symbol
					distributions.Length -= 1;
					resultPattern.AppendFormat("(?={0})", distributions);
				}

				if (negativeMatch.Length > 0)
				{
					// remove last '|' symbol
					negativeMatch.Length -= 1;
					resultPattern.AppendFormat("(?!{0})", negativeMatch);
				}

				if (positiveMatch.Length > 0)
				{
					// remove last '|' symbol
					positiveMatch.Length -= 1;
					resultPattern.Append(positiveMatch);
				}

				resultPattern.Append("$");

				// get time
				var date = DateTime.ParseExact(
					lastMatch.Groups["date"].Value, "yyMMdd HHmmss",
					null,	DateTimeStyles.AllowWhiteSpaces);
				if (lastMatch.Groups["timezone"].Success)
					// convert GMT to local time
					date = date.ToLocalTime();

				var articleList = session.DataProvider.GetArticleList(date, resultPattern.ToString());

				var textResponse = new StringBuilder();
				foreach (var article in articleList)
					textResponse.Append(article["Message-ID"]).Append(Util.CRLF);

				return new Response(NntpResponse.ListOfArticlesByMessageID, textResponse.ToString());
			}
			catch (ArgumentOutOfRangeException)
			{
				return new Response(NntpResponse.SyntaxisError); //wrong date/time
			}
		}
	}
}
