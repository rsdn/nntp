using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using derIgel.Utils;

[assembly:derIgel.NNTP.Commands.NNTPCommand("")]

namespace derIgel
{
	namespace NNTP
	{
		namespace Commands
		{
			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true,
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
						return new Response(501); // syntaxis error
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
			}

			/// <summary>
			/// XOVER client command
			/// </summary>
			[NNTPCommand("XOVER")]
			public class Xover : Generic
			{
				protected static Regex XoverSyntaxisChecker =
					new Regex(@"(?in)^XOVER([ \t]+(?<startNumber>\d+)([ \t]*(?<dash>-)[ \t]*(?<endNumber>\d+)?)?)?[ \t]*$",
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
						articleList = session.dataProvider.
							GetArticleList(startNumber, endNumber, NewsArticle.Content.Header);
					}
					else
					{
						articleList = new NewsArticle[1];
						articleList[0] = session.dataProvider.GetArticle(NewsArticle.Content.Header);
					}
					string output = string.Empty;
					foreach (NewsArticle article in articleList)
						output += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\r\n",
							article.Number,
							article.EncodedHeader("Subject"),
							article.EncodedHeader("From"),
							article["Date"],
							article["Message-ID"],
							article["References"],
							null, null, null);
					return new Response(224, null, output);
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
					new Regex(@"(?in)^(?<command>ARTICLE|HEAD|BODY|STAT)([ \t]+((?<messageID>\<\S+\>)|(?<messageNumber>\d+)))?[ \t]*$",
										RegexOptions.Compiled);

				public ArticleHeadBodyStat(Session session) : base(session)	
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = ArticleHeadBodyStatSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					int responseCode;
					NewsArticle.Content content;
					switch (lastMatch.Groups["command"].Value.ToUpper())
					{
						case "ARTICLE" :
							content = NewsArticle.Content.HeaderAndBody;
							responseCode = 220;
							break;
						case "HEAD"	:
							content = NewsArticle.Content.Header;
							responseCode = 221;
							break;
						case "BODY"	:
							content = NewsArticle.Content.Body;
							responseCode = 222;
							break;
						case "STAT"	:
							content = NewsArticle.Content.None;
							responseCode = 223;
							break;
						default:
							content = NewsArticle.Content.None;
							responseCode = 503;
							break;
					}

					NewsArticle article;
					if (lastMatch.Groups["messageID"].Success)
					{
						article = session.dataProvider.GetArticle(
							lastMatch.Groups["messageID"].Value, content);
					}
					else
						if (lastMatch.Groups["messageNumber"].Success)
						article = session.dataProvider.GetArticle(
							Convert.ToInt32(lastMatch.Groups["messageNumber"].Value),
							content);
					else
						article = session.dataProvider.GetArticle(NewsArticle.Content.HeaderAndBody);
					
					return new Response(responseCode,
						new string[]{article.Number.ToString(), article["Message-ID"]},
						article.ToString());
				}
			}

			/// <summary>
			/// NEXT client command
			/// </summary>
			[NNTPCommand("NEXT")]
			public class Next : Generic
			{
				protected static Regex NextSyntaxisChecker= new Regex(@"(?in)^NEXT[ \t]*$", RegexOptions.Compiled);

				public Next(Session session) : base(session)
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = NextSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					NewsArticle article = session.dataProvider.GetNextArticle();
					return new Response(223, new string[]{article.Number.ToString(),
																								 article["Message-ID"]});
				}
			}
		
			/// <summary>
			/// LAST client command
			/// </summary>
			[NNTPCommand("LAST")]
			public class Last : Generic
			{
				protected static Regex LastSyntaxisChecker = new Regex(@"(?in)^LAST[ \t]*$", RegexOptions.Compiled);

				public Last(Session session) : base(session)
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = LastSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					NewsArticle article = session.dataProvider.GetPrevArticle();
					return new Response(223, new string[]{article.Number.ToString(),
																								 article["Message-ID"]});
				}
			}

			/// <summary>
			/// GROUP client command
			/// </summary>
			[NNTPCommand("GROUP")]
			public class Group : Generic
			{
				protected static Regex GroupSyntaxisChecker = new Regex(@"(?in)^GROUP[ \t]+(?<groupName>\S+)[ \t]*$",
					RegexOptions.Compiled);

				public Group(Session session) : base(session)
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = GroupSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					string groupName = lastMatch.Groups["groupName"].Value;
					NewsGroup group = session.dataProvider.GetGroup(groupName);
					return new Response(211, new string[]{group.EtimatedArticles.ToString(),
																								 group.FirstArticleNumber.ToString(),
																								 group.LastArticleNumber.ToString(),
																								 groupName});
				}
			}

			/// <summary>
			/// LIST client command
			/// </summary>
			[NNTPCommand("LIST")]
			public class List : Generic
			{
				protected static Regex ListSyntaxisChecker = new	Regex(@"(?in)^LIST[ \t]*$", RegexOptions.Compiled);

				public List(Session session) : base(session)
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = ListSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					NewsGroup[] groupList = session.dataProvider.GetGroupList(new DateTime(), null);
					string textResponse = string.Empty;
					foreach (NewsGroup group in groupList)
						textResponse += string.Format("{0} {1} {2} {3}" + Util.CRLF,
							group.Name, group.LastArticleNumber, group.FirstArticleNumber,
							group.PostingAllowed ? 'y' : 'n');
					return new Response(215, null, textResponse);
				}
			}

			/// <summary>
			/// NEWGROUPS client command
			/// </summary>
			[NNTPCommand("NEWGROUPS")]
			public class NewGroups : Generic
			{
				protected static Regex NewGroupsSyntaxisChecker =
					new	Regex(@"(?in)^NEWGROUPS[ \t]+(?<date>\d{6}[ \t]+\d{6})([ \t]+(?<timezone>GMT))?([ \t]+<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
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
					
						NewsGroup[] groupList = session.dataProvider.GetGroupList(date, distributions);
						string textResponse = string.Empty;
						foreach (NewsGroup group in groupList)
							textResponse += string.Format("{0} {1} {2} {3}" + Util.CRLF,
								group.Name, group.LastArticleNumber, group.FirstArticleNumber,
								group.PostingAllowed ? 'y' : 'n');
						return new Response(231, null, textResponse);
					}
					catch (ArgumentOutOfRangeException)
					{
						return new Response(501); //wrong date/time
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
					new	Regex(@"(?in)^NEWNEWS[ \t]+(?<newsgroups>\w+(.\w+)*(,\w+(.\w+)*)*)[ \t]+(?<date>\d{6}[ \t]+\d{6})([ \t]+(?<timezone>GMT))?([ \t]+<(?<distributions>\w+(.\w+)*(,\w+(.\w+)*)*)>)?[ \t]*$",
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
					
						NewsArticle[] articleList = session.dataProvider.GetArticleList(newsgroups, date, distributions);
						string textResponse = string.Empty;
						foreach (NewsArticle article in articleList)
							textResponse += string.Format("{0}" + Util.CRLF, article["Message-ID"]);
						return new Response(230, null, textResponse);
					}
					catch (ArgumentOutOfRangeException)
					{
						return new Response(501); //wrong date/time
					}
				}
			}

			/// <summary>
			/// POST client command
			/// </summary>
			[NNTPCommand("POST")]
			public class Post : Generic
			{
				protected static Regex PostSyntaxisChecker = new	Regex(@"(?in)^POST[ \t]*$", RegexOptions.Compiled);

				public Post(Session session) : base(session)
				{
					allowedStates = Session.States.Normal;
					syntaxisChecker = PostSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					session.sessionState = Session.States.PostWaiting;
					return new Response(340);
				}
			}

			/// <summary>
			/// QUIT client command
			/// </summary>
			[NNTPCommand("QUIT")]
			public class Quit : Generic
			{
				protected static Regex QuitSyntaxisChecker = new Regex(@"(?in)^QUIT[ \t]*$", RegexOptions.Compiled);

				public Quit(Session session) : base(session)
				{
					syntaxisChecker = QuitSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					return new Response(205); // bye!
				}
			}

			/// <summary>
			/// SLAVE client command
			/// </summary>
			[NNTPCommand("SLAVE")]
			public class Slave : Generic
			{
				protected static Regex SlaveSyntaxisChecker = new	Regex(@"(?in)^SLAVE[ \t]*$", RegexOptions.Compiled);

				public Slave(Session session) : base(session)
				{
					syntaxisChecker = SlaveSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					return new Response(202); // ok
				}
			}

			/// <summary>
			/// MODE READER and MODE STREAM client command
			/// </summary>
			[NNTPCommand("MODE")]
			public class Mode : Generic
			{
				protected static Regex ModeSyntaxisChecker = new	Regex(@"(?in)^MODE[ \t]+(?<mode>READER|STREAM)[ \t]*$",
					RegexOptions.Compiled);

				public Mode(Session session) : base(session)
				{
					syntaxisChecker = ModeSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					Response result;
					if (lastMatch.Groups["mode"].Value.ToUpper() == "READER")
						// MODE READER
						result = new Response(session.dataProvider.PostingAllowed ? 200 : 201);
					else
						// MODE STREAM
						result = new Response(500);
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
								session.dataProvider.username	=	lastMatch.Groups["param"].Value;
								session.sessionState = Session.States.MoreAuthRequired;
								result = new Response(381);
								break;
							case Session.States.MoreAuthRequired	:
								result = new Response(482);
								break;
						}
					else
						// AUTHINFO PASS
						switch (session.sessionState)
						{
							case	Session.States.Normal	:
							case	Session.States.AuthRequired	:
								result = new Response(482);
								break;
							case Session.States.MoreAuthRequired	:
								session.dataProvider.password	=	lastMatch.Groups["param"].Value;
								if (session.dataProvider.Authentificate(session.dataProvider.username, session.dataProvider.password))
								{
									session.sessionState = Session.States.Normal;
									result = new Response(281);
								}
								else
								{
									session.sessionState = Session.States.AuthRequired;
									result = new Response(502);
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
				protected static Regex HelpSyntaxisChecker = new	Regex(@"(?in)^HELP[ \t]*$", RegexOptions.Compiled);

				public Help(Session session) : base(session)
				{
					syntaxisChecker = HelpSyntaxisChecker;
				}

				protected override Response ProcessCommand()
				{
					string supportCommands = "RSDN NNTP Sever support follow commands:" + Util.CRLF;
					foreach (string command in session.commands.Keys)
						supportCommands += '\t' + command + Util.CRLF;
					return new Response(100, null, supportCommands);
				}
			}
		}
	}
}