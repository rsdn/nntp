using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// NEWGROUPS command.
	/// </summary>
	[NntpCommand("NEWGROUPS")]
	public class NewGroups : Generic
	{
		/// <summary>
		/// Command syntaxis checker.
		/// </summary>
		protected static Regex NewGroupsSyntaxisChecker =
			new	Regex(@"(?in)^NEWGROUPS[ \t]+(?<date>\d{6}[ \t]+\d{6})([ \t]+(?<timezone>GMT))?" +
								@"([ \t]+<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
								RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public NewGroups(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NewGroupsSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP Resaponse.</returns>
		protected override Response ProcessCommand()
		{
			try
			{
				// get time
				var date = DateTime.ParseExact(lastMatch.Groups["date"].Value, "yyMMdd HHmmss",
					null,	DateTimeStyles.AllowWhiteSpaces);
				if (lastMatch.Groups["timezone"].Success)
					// convert GMT to local time
					date = date.ToLocalTime();

				// get distributions if exist
				string distributions = null;
				if (lastMatch.Groups["distributions"].Success)
				{
					foreach (var pattern in lastMatch.Groups["distributions"].Value.Split(new[]{','}))
						distributions += string.Format("{0}.*|", TransformWildmat(pattern));
					// remove last |
					distributions = distributions.Substring(0, distributions.Length - 1);
				}

				var groupList = session.DataProvider.GetGroupList(date, distributions);
				var textResponse = new StringBuilder();
				foreach (var group in groupList)
					textResponse.AppendFormat("{0} {1} {2} {3}",
						group.Name, group.LastArticleNumber, group.FirstArticleNumber, group.PostingAllowed ? 'y' : 'n').
						Append(Util.CRLF);
				return new Response(NntpResponse.ListOfArticles, textResponse.ToString());
			}
			catch (ArgumentOutOfRangeException)
			{
				return new Response(NntpResponse.SyntaxisError); //wrong date/time
			}
		}
	}
}
