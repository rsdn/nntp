using System;
using System.Text.RegularExpressions;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// GROUP command.
	/// </summary>
	[NntpCommand("GROUP")]
	public class Group : Generic
	{
		/// <summary>
		/// Regex syntaxis checker for GROUP command.
		/// </summary>
		protected static Regex GroupSyntaxisChecker =
			new Regex(@"(?in)^GROUP[ \t]+(?<groupName>\S+)[ \t]*$",	RegexOptions.Compiled);

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">NNTP parent session.</param>
		public Group(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = GroupSyntaxisChecker;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response</returns>
		protected override Response ProcessCommand()
		{
			string groupName = lastMatch.Groups["groupName"].Value;
			NewsGroup group = session.DataProvider.GetGroup(groupName);
			
			// set cuurent pointers for group and article
			session.currentGroup = groupName;
			session.currentArticle = group.FirstArticleNumber;

			return new Response(NntpResponse.GroupSelected, null, group.EtimatedArticles, group.FirstArticleNumber,
				group.LastArticleNumber, groupName);
		}
	}
}
