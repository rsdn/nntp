using System;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace derIgel
{
	namespace NNTP
	{
		namespace Commands
		{
			[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
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
					this.syntaxisChecker = null;
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
			}

			/// <summary>
			/// XOVER client command
			/// </summary>
			[NNTPCommand("XOVER")]
			public class Xover : Generic
			{
				public Xover(Session session) : base(session)
				{
					syntaxisChecker = new Regex(@"(?i)^XOVER(( |\t)+(?<startNumber>\d+)(\-(?<endNumber>\d+))?)?( |\t)*$");
				}

				protected override Response ProcessCommand()
				{
					NewsArticle[] articleList;
					if (lastMatch.Groups["startNumber"].Success)
					{
						int startNumber = Convert.ToInt32(lastMatch.Groups["startNumber"].Value);
						int endNumber = startNumber;
						if (lastMatch.Groups["endNumber"].Success)
						{
							endNumber = Convert.ToInt32(lastMatch.Groups["endNumber"].Value);
						}
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
				public ArticleHeadBodyStat(Session session) : base(session)	
				{
					syntaxisChecker = new Regex(@"(?i)^(?<command>ARTICLE|HEAD|BODY|STAT)(( |\t)+((?<messageID><\S*>)|(?<messageNumber>\d+)))?( |\t)*$");
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

//			/// <summary>
//			/// ARTICLE client command
//			/// </summary>
//			[NNTPCommand("ARTICLE")]
//			public class Article : BaseArticle
//			{
//				public Article(Session session) : base(session)
//				{
//					syntaxisChecker = new Regex(@"(?i)^ARTICLE( |\t)*((?<messageID><\S*>)|(?<messageNumber>[0-9]+))?( |\t)*$");
//				}
//
//				protected override Response ProcessCommand()
//				{
//					return ProcessCommand(NewsArticle.Content.HeaderAndBody);
//				}
//			}
//
//			/// <summary>
//			/// HEAD client command
//			/// </summary>
//			[NNTPCommand("HEAD")]
//			public class Head : BaseArticle
//			{
//				public Head(Session session) : base(session)
//				{
//					syntaxisChecker = new Regex(@"(?i)^HEAD( |\t)*((?<messageID><\S*>)|(?<messageNumber>[0-9]+))?( |\t)*$");
//				}
//
//				protected override Response ProcessCommand()
//				{
//					return ProcessCommand(NewsArticle.Content.Header);
//				}
//			}
//
//			/// <summary>
//			/// BODY client command
//			/// </summary>
//			[NNTPCommand("BODY")]
//			public class Body : BaseArticle
//			{
//				public Body(Session session) : base(session)
//				{
//					syntaxisChecker = new Regex(@"(?i)^BODY( |\t)*((?<messageID><\S*>)|(?<messageNumber>[0-9]+))?( |\t)*$");
//				}
//
//				protected override Response ProcessCommand()
//				{
//					return ProcessCommand(NewsArticle.Content.Body);
//				}
//			}
//
//			/// <summary>
//			/// STAT client command
//			/// </summary>
//			[NNTPCommand("STAT")]
//			public class Stat : BaseArticle
//			{
//				public Stat(Session session) : base(session)
//				{
//					syntaxisChecker = new Regex(@"(?i)^STAT( |\t)*((?<messageID><\S*>)|(?<messageNumber>[0-9]+))?( |\t)*$");
//				}
//
//				protected override Response ProcessCommand()
//				{
//					return ProcessCommand(NewsArticle.Content.None);
//				}
//			}

			/// <summary>
			/// NEXT client command
			/// </summary>
			[NNTPCommand("NEXT")]
			public class Next : Generic
			{
				public Next(Session session) : base(session)
				{
					syntaxisChecker = new Regex(@"(?i)^NEXT( |\t)*$");
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
				public Last(Session session) : base(session)
				{
					syntaxisChecker = new Regex(@"(?i)^LAST( |\t)*$");
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
				public Group(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^GROUP( |\t)*(?<groupName>\S+)( |\t)*$");
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
				public List(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^LIST( |\t)*$");
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
				public NewGroups(Session session) : base(session)
				{
					syntaxisChecker =
						new	Regex(@"(?i)^NEWGROUPS( |\t)+(?<date>[0-9][0-9][0-1][0-9][0-3][0-9]( |\t)+[0-2][0-9][0-5][0-9][0-5][0-9])( |\t)*(?<timezone>GMT)?( |\t)*(<(?<distributions>((\w+\.?)+,?)+)>)?( |\t)*$");
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
				public NewNews(Session session) : base(session)
				{
					syntaxisChecker =
						new	Regex(@"(?i)^NEWNEWS( |\t)+(?<newsgroups>((\!?(\w+|\*)\.?)+,?)+)( |\t)+(?<date>[0-9][0-9][0-1][0-9][0-3][0-9]( |\t)+[0-2][0-9][0-5][0-9][0-5][0-9])( |\t)*(?<timezone>GMT)?( |\t)*(<(?<distributions>((\w+\.?)+,?)+)>)?( |\t)*$");
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
				public Post(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^POST( |\t)*$");
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
				public Quit(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^QUIT( |\t)*$");
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
				public Slave(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^SLAVE( |\t)*$");
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
				public Mode(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^MODE( |\t)+(?<mode>READER|STREAM)( |\t)*$");
				}

				protected override Response ProcessCommand()
				{
					Response result;
					if (lastMatch.Groups["mode"].Value.ToUpper() == "READER")
						// MODE READER
						result = new Response(201);
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
				public AuthInfo(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^AUTHINFO( |\t)+(?<mode>USER|PASS)( |\t)*(?<param>\w+)( |\t)*$");
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
				public Help(Session session) : base(session)
				{
					syntaxisChecker = new	Regex(@"(?i)^HELP( |\t)*$");
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