using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using derIgel.NNTP;
using System.Text;
using derIgel.MIME;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Net;

[assembly:derIgel.NNTP.Commands.NNTPCommand("")]

namespace derIgel.NNTP.Commands
{
	/// See RFC  977 'Network News Transport Protocol'
	/// See RFC 2980 'Common NNTP Extensions'
	
	using Util = derIgel.MIME.Util;
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false,
			AllowMultiple = true)]
	public class NNTPCommandAttribute : Attribute
	{
		public NNTPCommandAttribute(string commandName)
		{
			command = commandName;
		}
		protected string command;

		internal string Name
		{
			get
			{
				return command;
			}
		}
	}

	/// <summary>
	/// Generic NNTP client command
	/// </summary>
	public abstract class Generic
	{
		public Generic(Session session)
		{
			syntaxisChecker = null;
			this.session = session;
		}

		/// <summary>
		/// Process client command
		/// </summary>
		public Response Process()
		{
			lastMatch = syntaxisChecker.Match(session.commandString);
			if (lastMatch.Success)
				return ProcessCommand();
			else
				return new Response(NntpResponse.SyntaxisError); // syntaxis error
		}

		protected abstract Response ProcessCommand();

		protected Regex syntaxisChecker;
		/// <summary>
		/// parent NNTP Session
		/// </summary>
		protected Session session;
		protected Match lastMatch;
		protected Session.States allowedStates;
		protected Session.States prohibitedStates;

		public bool IsAllowed(Session.States state)
		{
			// not prohibited 
			if ((prohibitedStates & state) == 0)
				// if any states explixity allowed
				if ((allowedStates ^ Session.States.None) != 0)
					return (allowedStates & state) != 0;
				else
					// else implicity allowed
					return true;

			return false;
		}

		/// <summary>
		/// add dditional headers specified by server
		/// </summary>
		/// <param name="article">news article</param>
		/// <returns>modified news article</returns>
		protected NewsArticle ModifyArticle(NewsArticle article)
		{
			StringBuilder xref = new StringBuilder(Session.hostname);
			foreach (DictionaryEntry newsGroupNumber in article.MessageNumbers)
				xref.Append(" ").Append(newsGroupNumber.Key).Append(":").Append(newsGroupNumber.Value);
			article["Xref"] = xref.ToString();
			article["X-Server"] = Manager.ServerID;
			return article;
		}
	}

	/// <summary>
	/// XOVER client command
	/// </summary>
	[NNTPCommand("XOVER")]
	public class Xover : Generic
	{
		protected static Regex XoverSyntaxisChecker =
			new Regex(@"(?in)^XOVER([ \t]+(?<startNumber>\d+)" + 
								@"([ \t]*(?<dash>-)[ \t]*(?<endNumber>\d+)?)?)?[ \t]*$",
								RegexOptions.Compiled);

		public Xover(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = XoverSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			NewsArticle[] articleList;
			if (lastMatch.Groups["startNumber"].Success)
			{
				int startNumber = Convert.ToInt32(lastMatch.Groups["startNumber"].Value);
				int endNumber = startNumber;
				if (lastMatch.Groups["dash"].Success)
					if (lastMatch.Groups["endNumber"].Success)
						endNumber = Convert.ToInt32(lastMatch.Groups["endNumber"].Value);
					else
						endNumber = -1;
				articleList = session.DataProvider.
					GetArticleList(startNumber, endNumber, NewsArticle.Content.HeaderAndBody);
			}
			else
			{
				articleList = new NewsArticle[1];
				articleList[0] = session.DataProvider.GetArticle(NewsArticle.Content.HeaderAndBody);
			}
			StringBuilder output = new StringBuilder();
			foreach (NewsArticle article in articleList)
			{
				output.Append(ModifyArticle(article).MessageNumbers[session.DataProvider.CurrentGroup]);
				foreach (string headerItem in List.headerItems)
					// replace in header all '\t', '\r', '\n' symbols to space
					output.Append('\t').Append(article[headerItem] == null ? null :
						article.EncodedHeader(headerItem).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '));
				output.Append(Util.CRLF);
			}
			return new Response(NntpResponse.Overview, Encoding.ASCII.GetBytes(output.ToString()));
		}
	}

	/// <summary>
	/// Common class for ARTICLE, HEAD, BODY & STAT client commands
	/// </summary>
	[NNTPCommand("ARTICLE")]
	[NNTPCommand("HEAD")]
	[NNTPCommand("BODY")]
	[NNTPCommand("STAT")]
	public class ArticleHeadBodyStat : Generic
	{
		protected static Regex ArticleHeadBodyStatSyntaxisChecker =
			new Regex(@"(?in)^(?<command>ARTICLE|HEAD|BODY|STAT)" + 
								@"([ \t]+((?<messageID>\<\S+\>)|(?<messageNumber>\d+)))?[ \t]*$",
								RegexOptions.Compiled);

		public ArticleHeadBodyStat(Session session) : base(session)	
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = ArticleHeadBodyStatSyntaxisChecker;
		}

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
				article = session.DataProvider.GetArticle(
					Convert.ToInt32(lastMatch.Groups["messageNumber"].Value),
					content);
			else
				article = session.DataProvider.GetArticle(NewsArticle.Content.HeaderAndBody);
			
			ModifyArticle(article);
			return new Response(responseCode, article.GetBody(),
				// article retirived by messageID don't change "internal current pointer", so we may don't have current group
				lastMatch.Groups["messageID"].Success ? null : article.MessageNumbers[session.DataProvider.CurrentGroup],
				article["Message-ID"]);
		}
	}

	/// <summary>
	/// NEXT client command
	/// </summary>
	[NNTPCommand("NEXT")]
	public class Next : Generic
	{
		protected static Regex NextSyntaxisChecker =
			new Regex(@"(?in)^NEXT[ \t]*$",	RegexOptions.Compiled);

		public Next(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NextSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			NewsArticle article = session.DataProvider.GetNextArticle();
			return new Response(NntpResponse.ArticleNothingRetrivied, null,
				article.MessageNumbers[session.DataProvider.CurrentGroup], article["Message-ID"]);
		}
	}

	/// <summary>
	/// LAST client command
	/// </summary>
	[NNTPCommand("LAST")]
	public class Last : Generic
	{
		protected static Regex LastSyntaxisChecker =
			new Regex(@"(?in)^LAST[ \t]*$", RegexOptions.Compiled);

		public Last(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = LastSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			NewsArticle article = session.DataProvider.GetPrevArticle();
			return new Response(NntpResponse.ArticleNothingRetrivied, null,
				article.MessageNumbers[session.DataProvider.CurrentGroup], article["Message-ID"]);
		}
	}

	/// <summary>
	/// GROUP client command
	/// </summary>
	[NNTPCommand("GROUP")]
	public class Group : Generic
	{
		protected static Regex GroupSyntaxisChecker =
			new Regex(@"(?in)^GROUP[ \t]+(?<groupName>\S+)[ \t]*$",	RegexOptions.Compiled);

		public Group(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = GroupSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			string groupName = lastMatch.Groups["groupName"].Value;
			NewsGroup group = session.DataProvider.GetGroup(groupName);
			return new Response(NntpResponse.GroupSelected, null, group.EtimatedArticles, group.FirstArticleNumber,
				group.LastArticleNumber, groupName);
		}
	}

	/// <summary>
	/// LIST, LIST NEWGROUPS, & LIST OVERVIEW.FMT client's commands
	/// </summary>
	[NNTPCommand("LIST")]
	public class List : Generic
	{
		public static readonly StringCollection headerItems = new StringCollection();

		protected static Regex ListSyntaxisChecker =
			new	Regex(@"(?in)^LIST([ \t]+((?<wideFormat>NEWSGROUPS)|(?<overview>OVERVIEW\.FMT)))?[ \t]*$",
			RegexOptions.Compiled);

		static List()
		{
			headerItems.Add("subject");
			headerItems.Add("from");
			headerItems.Add("date");
			headerItems.Add("message-id");
			headerItems.Add("references");
			headerItems.Add("bytes");
			headerItems.Add("lines");
			headerItems.Add("xref");
		}
		public List(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = ListSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			StringBuilder textResponse = new StringBuilder();
			if (lastMatch.Groups["overview"].Success)
				// overview format
				foreach (string headerItem in headerItems)
					textResponse.Append(headerItem).Append(Util.CRLF);
			else
			{
				NewsGroup[] groupList = session.DataProvider.GetGroupList(new DateTime(), null);

				if (lastMatch.Groups["wideFormat"].Success)
					// wide format
					foreach (NewsGroup group in groupList)
						textResponse.AppendFormat("{0} {1}{2}",
							group.Name, group.Description, Util.CRLF);
				else
					// standart format
					foreach (NewsGroup group in groupList)
						textResponse.AppendFormat("{0} {1} {2} {3}{4}",
							group.Name, group.LastArticleNumber, group.FirstArticleNumber,
							group.PostingAllowed ? 'y' : 'n', Util.CRLF);
			}
			return new Response(NntpResponse.ListOfGroups, Encoding.ASCII.GetBytes(textResponse.ToString()));
		}
	}

	/// <summary>
	/// NEWGROUPS client command
	/// </summary>
	[NNTPCommand("NEWGROUPS")]
	public class NewGroups : Generic
	{
		protected static Regex NewGroupsSyntaxisChecker =
			new	Regex(@"(?in)^NEWGROUPS[ \t]+(?<date>\d{6}[ \t]+\d{6})" +
								@"([ \t]+(?<timezone>GMT))?([ \t]+" +
								@"<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
								RegexOptions.Compiled);

		public NewGroups(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NewGroupsSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			try
			{
				// get time
				DateTime date = DateTime.ParseExact(
					lastMatch.Groups["date"].Value, "yyMMdd HHmmss",
					null,	System.Globalization.DateTimeStyles.AllowWhiteSpaces);
				if (!lastMatch.Groups["timezone"].Success)
					// convert local time to GMT
					date = TimeZone.CurrentTimeZone.ToUniversalTime(date);

				// get distributions if exist
				string[] distributions = null;
				string distr = lastMatch.Groups["distributions"].Value;
				if (lastMatch.Groups["distributions"].Success)
					distributions =
						lastMatch.Groups["distributions"].Value.
						Split(new char[]{','});
			
				NewsGroup[] groupList = session.DataProvider.GetGroupList(date, distributions);
				StringBuilder textResponse = new StringBuilder();
				foreach (NewsGroup group in groupList)
					textResponse.AppendFormat("{0} {1} {2} {3}{4}",
						group.Name, group.LastArticleNumber, group.FirstArticleNumber,
						group.PostingAllowed ? 'y' : 'n', Util.CRLF);
				return new Response(NntpResponse.ListOfArticles, Encoding.ASCII.GetBytes(textResponse.ToString()));
			}
			catch (ArgumentOutOfRangeException)
			{
				return new Response(NntpResponse.SyntaxisError); //wrong date/time
			}
		}
	}

	/// <summary>
	/// NEWNEWS client command
	/// </summary>
	[NNTPCommand("NEWNEWS")]
	public class NewNews : Generic
	{
		protected static Regex NewNewsSyntaxisChecker =
			new	Regex(@"(?in)^NEWNEWS[ \t]+(?<newsgroups>\w+(.\w+)*(,\w+(.\w+)*)*)[ \t]+" + 
								@"(?<date>\d{6}[ \t]+\d{6})([ \t]+(?<timezone>GMT))?([ \t]+" + 
								@"<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
								RegexOptions.Compiled);

		public NewNews(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = NewNewsSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			try
			{
				// get newsgroup names
				string[] newsgroups = null;
				if (lastMatch.Groups["newsgroups"].Success)
					newsgroups =
						lastMatch.Groups["newsgroups"].Value.
						Split(new char[]{','});

				// get time
				DateTime date = DateTime.ParseExact(
					lastMatch.Groups["date"].Value, "yyMMdd HHmmss",
					null,	System.Globalization.DateTimeStyles.AllowWhiteSpaces);
				if (!lastMatch.Groups["timezone"].Success)
					// convert local time to GMT
					date = TimeZone.CurrentTimeZone.ToUniversalTime(date);

				// get distributions if exist
				string[] distributions = null;
				if (lastMatch.Groups["distributions"].Success)
					distributions =
						lastMatch.Groups["distributions"].Value.
						Split(new char[]{','});
			
				NewsArticle[] articleList =
					session.DataProvider.GetArticleList(newsgroups, date, distributions);
				StringBuilder textResponse = new StringBuilder();
				foreach (NewsArticle article in articleList)
					textResponse.AppendFormat("{0}{1}", article["Message-ID"],
						Util.CRLF);
				return new Response(NntpResponse.ListOfArticlesByMessageID,
					Encoding.ASCII.GetBytes(textResponse.ToString()));
			}
			catch (ArgumentOutOfRangeException)
			{
				return new Response(NntpResponse.SyntaxisError); //wrong date/time
			}
		}
	}

	/// <summary>
	/// POST client command
	/// </summary>
	[NNTPCommand("POST")]
	public class Post : Generic
	{
		protected static Regex PostSyntaxisChecker =
			new Regex(@"(?in)^POST[ \t]*$", RegexOptions.Compiled);

		public Post(Session session) : base(session)
		{
			allowedStates = Session.States.Normal;
			syntaxisChecker = PostSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			session.sessionState = Session.States.PostWaiting;
			return new Response(NntpResponse.SendArticle);
		}
	}

	/// <summary>
	/// QUIT client command
	/// </summary>
	[NNTPCommand("QUIT")]
	public class Quit : Generic
	{
		protected static Regex QuitSyntaxisChecker =
			new Regex(@"(?in)^QUIT[ \t]*$", RegexOptions.Compiled);

		public Quit(Session session) : base(session)
		{
			syntaxisChecker = QuitSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Bye);
		}
	}

	/// <summary>
	/// SLAVE client command
	/// </summary>
	[NNTPCommand("SLAVE")]
	public class Slave : Generic
	{
		protected static Regex SlaveSyntaxisChecker =
			new	Regex(@"(?in)^SLAVE[ \t]*$", RegexOptions.Compiled);

		public Slave(Session session) : base(session)
		{
			syntaxisChecker = SlaveSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			return new Response(NntpResponse.Slave); // ok
		}
	}

	/// <summary>
	/// MODE READER and MODE STREAM client command
	/// </summary>
	[NNTPCommand("MODE")]
	public class Mode : Generic
	{
		protected static Regex ModeSyntaxisChecker =
			new Regex(@"(?in)^MODE[ \t]+(?<mode>READER|STREAM)[ \t]*$",	RegexOptions.Compiled);

		public Mode(Session session) : base(session)
		{
			syntaxisChecker = ModeSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			Response result;
			if (lastMatch.Groups["mode"].Value.ToUpper() == "READER")
				// MODE READER
				result = new Response(session.DataProvider.PostingAllowed ? NntpResponse.Ok : NntpResponse.OkNoPosting,
					null, Manager.ServerID);
			else
				// MODE STREAM
				result = new Response(NntpResponse.NotRecognized);
			return result;
		}
	}

	/// <summary>
	/// AUTHINFO USER and AUTHINFO PASS client commands
	/// </summary>
	[NNTPCommand("AUTHINFO")]
	public class AuthInfo : Generic
	{
		protected static Regex AuthInfoSyntaxisChecker = 
			new	Regex(@"(?in)^AUTHINFO[ \t]+(?<mode>USER|PASS)[ \t]+(?<param>\w+)[ \t]*$",
								RegexOptions.Compiled);

		public AuthInfo(Session session) : base(session)
		{
			syntaxisChecker = AuthInfoSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			Response result = null;
			if (lastMatch.Groups["mode"].Value.ToUpper() == "USER")
				// AUTHINFO USER
				switch (session.sessionState)
				{
					case	Session.States.Normal	:
					case	Session.States.AuthRequired	:
						session.Username	=	lastMatch.Groups["param"].Value;
						session.sessionState = Session.States.MoreAuthRequired;
						result = new Response(NntpResponse.MoreAuthentificationRequired);
						break;
					case Session.States.MoreAuthRequired	:
						session.sessionState = Session.States.AuthRequired;
						session.Username = "";
						result = new Response(NntpResponse.AuthentificationRejected);
						break;
				}
			else
				// AUTHINFO PASS
				switch (session.sessionState)
				{
					case	Session.States.Normal	:
					case	Session.States.AuthRequired	:
						result = new Response(NntpResponse.AuthentificationRejected);
						break;
					case Session.States.MoreAuthRequired	:
						session.Password	=	lastMatch.Groups["param"].Value;
						if (session.DataProvider.Authentificate(session.Username, session.Password))
						{
							session.sessionState = Session.States.Normal;
							result = new Response(281);
							string remoteHost = ((IPEndPoint)session.client.RemoteEndPoint).Address.ToString();
							try
							{
								remoteHost = Dns.GetHostByAddress(remoteHost).HostName;
							}
							catch (SocketException) {}
							session.sender = session.Username + "@" + remoteHost;
						}
						else
						{
							session.Username = "";
							session.Password = "";
							session.sessionState = Session.States.AuthRequired;
							result = new Response(NntpResponse.NoPermission);
							session.sender = null;
						}
						break;
				}
			return result;
		}
	}

	/// <summary>
	/// HELP client command
	/// </summary>
	[NNTPCommand("HELP")]
	public class Help : Generic
	{
		protected static Regex HelpSyntaxisChecker =
			new Regex(@"(?in)^HELP[ \t]*$", RegexOptions.Compiled);

		public Help(Session session) : base(session)
		{
			syntaxisChecker = HelpSyntaxisChecker;
		}

		protected override Response ProcessCommand()
		{
			StringBuilder supportCommands = new StringBuilder();
			supportCommands.Append(Manager.ServerID).Append(" ").
				Append("supports follow commands:").Append(Util.CRLF);
			foreach (string command in session.commands.Keys)
				supportCommands.AppendFormat("\t{0}{1}", command,Util.CRLF);
			return new Response(NntpResponse.Help, Encoding.ASCII.GetBytes(supportCommands.ToString()));
		}
	}
}