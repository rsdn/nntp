using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// Generic NNTP command
	/// See RFC  977 'Network News Transport Protocol'
	/// See RFC 2980 'Common NNTP Extensions'
	/// </summary>
	public abstract class Generic
	{
#if PERFORMANCE_COUNTERS
		/// <summary>
		/// RSDN NNTP Command performance counters' category.
		/// </summary>
		public const string CommandCategoryName = "RSDN NNTP Command";
#endif

		/// <summary>
		/// Create command handler for specific session.
		/// </summary>
		/// <param name="session">Parent NNTP session.</param>
		public Generic(Session session)
		{
			syntaxisChecker = null;
			this.session = session;
		}

		/// <summary>
		/// Process command.
		/// </summary>
		public Response Process()
		{
			lastMatch = syntaxisChecker.Match(session.commandString);
			if (lastMatch.Success)
				return ProcessCommand();
			else
				return new Response(NntpResponse.SyntaxisError); // syntaxis error
		}

		/// <summary>
		/// Function doing processing of command.
		/// Virtual member function must be overrided in inherited classes. 
		/// </summary>
		/// <returns></returns>
		protected abstract Response ProcessCommand();

		/// <summary>
		/// Regex checker for command syntaxis.
		/// </summary>
		protected Regex syntaxisChecker;
		/// <summary>
		/// Regex match object after syntaxis checking.
		/// </summary>
		protected Match lastMatch;

		/// <summary>
		/// parent NNTP Session
		/// </summary>
		protected Session session;

		/// <summary>
		/// Prohibited states for this command (have priority over allowed states).
		/// </summary>
		protected Session.States prohibitedStates;
		/// <summary>
		/// Explicit allowed states for this command.
		/// </summary>
		protected Session.States allowedStates;

		/// <summary>
		/// Check if command is allowed in specified state.
		/// </summary>
		/// <param name="state">Necessary state.</param>
		/// <returns>True if allowed.</returns>
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
		/// Identification string for this assembly.
		/// </summary>
		protected static readonly string nntpID = Assembly.GetExecutingAssembly().GetName().Name + " " +
			Assembly.GetExecutingAssembly().GetName().Version;

		/// <summary>
		/// Post-processing of command.
		/// Adding dditional headers specified by server.
		/// </summary>
		/// <param name="article">news article</param>
		/// <returns>modified news article</returns>
		protected virtual NewsArticle ModifyArticle(NewsArticle article)
		{
			StringBuilder xref = new StringBuilder(Session.Hostname);
			foreach (DictionaryEntry newsGroupNumber in article.MessageNumbers)
				xref.Append(" ").Append(newsGroupNumber.Key).Append(":").Append(newsGroupNumber.Value);
			article["Xref"] = xref.ToString();
			article["X-Server"] = string.Join("; ", new string[]{Manager.ServerID, nntpID, session.DataProvider.Identity});
			return article;
		}

		/// <summary>
		/// Trasnform WILDMAT Unix pattern format to regex pattern string.
		/// ATTENTION! Realized not fully.
		/// </summary>
		/// <param name="pattern"></param>
		/// <returns></returns>
		protected string TransformWildmat(string pattern)
		{
			if (pattern == null)
				throw new ArgumentNullException("pattern");

			string regexPattern = pattern;
			
			// Temporary remove '?' characters
			regexPattern = Regex.Replace(regexPattern, @"(?<!\\)\?", "<<quest>>");
			// Temporary remove '*' characters
			regexPattern = Regex.Replace(regexPattern, @"(?<!\\)\*", "<<star>>");
			// Escape non allowed characters
			regexPattern = Regex.Escape(regexPattern);
			// Process '?' characters
			regexPattern = regexPattern.Replace("<<quest>>", ".?");
			// Process '*' characters
			regexPattern = regexPattern.Replace("<<star>>", ".*");

			return regexPattern;
		}
	}

}
