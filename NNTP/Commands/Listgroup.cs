using System.Text;
using System.Text.RegularExpressions;
using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// LISTGROUP command.
	/// The LISTGROUP command is used to get a listing of all the article numbers in a particular news group.
	/// </summary>
	[NntpCommand("LISTGROUP")]
	public class Listgroup : Generic
	{
		/// <summary>
		/// Regex syntaxis checker for LISTGROUP command.
		/// </summary>
		protected static Regex ListgroupSyntaxisChecker =
			new Regex(@"(?in)^LISTGROUP([ \t]+(?<groupName>\S+))?[ \t]*$",	RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">NNTP parent session.</param>
		public Listgroup(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = ListgroupSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			string groupName;

			if (!lastMatch.Groups["groupName"].Success)
			{
				if (session.currentGroup == null)
					throw new DataProviderException(DataProviderErrors.NoSelectedGroup);
				groupName = session.currentGroup;
			}
			else
				groupName = lastMatch.Groups["groupName"].Value;

			var group = session.DataProvider.GetGroup(groupName);

			var listOfNumbers = new StringBuilder();
			foreach (var article in
				session.DataProvider.GetArticleList(group.FirstArticleNumber, group.LastArticleNumber, groupName,NewsArticle.Content.Header))
					listOfNumbers.Append(article.MessageNumbers[groupName]).Append(Util.CRLF);

			return new Response("list of article numbers follow", NntpResponse.GroupSelected, listOfNumbers.ToString());
		}
	}
}
