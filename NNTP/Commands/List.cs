using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

using Rsdn.Mime;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// LIST, LIST NEWGROUPS, & LIST OVERVIEW.FMT commands.
	/// </summary>
	[NntpCommand("LIST")]
	public class List : Generic
	{
		/// <summary>
		/// List of headers for LIST OVERVIEW.FMT command.
		/// </summary>
		public static readonly StringCollection headerItems = new StringCollection();

		protected static Regex ListSyntaxisChecker =
			new	Regex(@"(?in)^LIST([ \t]+((?<wideFormat>NEWSGROUPS([ \t]+(?<wildmat>\S+))?)|(?<overview>OVERVIEW\.FMT)|(?<active>ACTIVE\.TIMES)))?[ \t]*$",
			RegexOptions.Compiled);

		/// <summary>
		/// Initialize list of headers.
		/// </summary>
		static List()
		{
			headerItems.Add("Subject");
			headerItems.Add("From");
			headerItems.Add("Date");
			headerItems.Add("Message-ID");
			headerItems.Add("References");
			headerItems.Add("Bytes");
			headerItems.Add("Lines");
			headerItems.Add("Xref");
		}

		/// <summary>
		/// Create command handler.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public List(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = ListSyntaxisChecker;
		}

		protected static DateTime unixStartDate = new DateTime(1970, 1, 1);

		/// <summary>
		/// Process command.
		/// </summary>
		/// <returns>Server's NNTP response.</returns>
		protected override Response ProcessCommand()
		{
			StringBuilder textResponse = new StringBuilder();
			if (lastMatch.Groups["overview"].Success)
				// overview format
				foreach (string headerItem in headerItems)
					textResponse.Append(headerItem).Append(':').Append(Util.CRLF);
			else if (lastMatch.Groups["active"].Success)
				// active.times
				foreach (NewsGroup group in session.DataProvider.GetGroupList(DateTime.MinValue, null))
				{
					TimeSpan period = group.Created - unixStartDate;
					textResponse.AppendFormat("{0} {1} admin@rsdn.ru", group.Name, (int)period.TotalSeconds).
						Append(Util.CRLF);
				}
			else
			{
				string pattern = null;
				if (lastMatch.Groups["wildmat"].Success)
					pattern = TransformWildmat(lastMatch.Groups["wildmat"].Value);

				NewsGroup[] groupList = session.DataProvider.GetGroupList(new DateTime(), pattern);

				if (lastMatch.Groups["wideFormat"].Success)
					// wide format
					foreach (NewsGroup group in groupList)
						textResponse.AppendFormat("{0} {1}", group.Name, group.Description).Append(Util.CRLF);
				else
					// standart format
					foreach (NewsGroup group in groupList)
						textResponse.AppendFormat("{0} {1} {2} {3}",
							group.Name, group.LastArticleNumber, group.FirstArticleNumber, group.PostingAllowed ? 'y' : 'n').
							Append(Util.CRLF);
			}
			return new Response(NntpResponse.ListOfGroups, textResponse.ToString());
		}
	}
}
