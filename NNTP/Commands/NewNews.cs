using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

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
			new	Regex(@"(?in)^NEWNEWS[ \t]+(?<newsgroups>\w+(.\w+)*(,\w+(.\w+)*)*)[ \t]+" + 
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
				StringCollection patterns = new StringCollection();

				// get newsgroup names
				string positiveMatch = null;
				string negativeMatch = null;
				foreach (string group in lastMatch.Groups["newsgroups"].Value.Split(new char[]{','}))
					if (group.StartsWith("!"))
						negativeMatch += string.Format("{0}|", TransformWildmat(group.Substring(1, group.Length - 1)));
					else
						positiveMatch += string.Format("{0}|", TransformWildmat(group.Substring(1, group.Length - 1)));

				if (positiveMatch != null)
					patterns.Add(positiveMatch.Substring(0, positiveMatch.Length - 1));
				if (negativeMatch != null)
					patterns.Add(string.Format("^(?!{0})", negativeMatch.Substring(0, negativeMatch.Length - 1)));

				// get time
				DateTime date = DateTime.ParseExact(
					lastMatch.Groups["date"].Value, "yyMMdd HHmmss",
					null,	System.Globalization.DateTimeStyles.AllowWhiteSpaces);
				if (lastMatch.Groups["timezone"].Success)
					// convert GMT to local time
					date = date.ToLocalTime();

				// get distributions if exist
				string distributions = null;
				if (lastMatch.Groups["distributions"].Success)
				{
					foreach (string pattern in lastMatch.Groups["distributions"].Value.Split(new char[]{','}))
						distributions += string.Format("{0}.*|", TransformWildmat(pattern));
					// remove last |
					distributions = distributions.Substring(0, distributions.Length - 1);
					patterns.Add(distributions);
				}
			
				string[] patternsArray = new string[patterns.Count];
				patterns.CopyTo(patternsArray, 0);
				NewsArticle[] articleList = session.DataProvider.GetArticleList(date, patternsArray);

				StringBuilder textResponse = new StringBuilder();
				foreach (NewsArticle article in articleList)
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
