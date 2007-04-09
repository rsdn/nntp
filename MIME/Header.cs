using System;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rsdn.Mime
{
	/// <summary>
	/// delegate for header filter processing
	/// </summary>
	public delegate string FilterHandler(string headerField, string value);

	/// <summary>
	/// Summary description for Header.
	/// </summary>
	[Serializable]
	public class Header : NameValueCollection
	{
		/// <summary>
		/// Hashtable for header identities' filters
		/// </summary>
		protected IDictionary<string, SortedList<int, FilterHandler>> filters;

		/// <summary>
		/// Construct empty header.
		/// </summary>
		public Header()
		{
			filters = new Dictionary<string, SortedList<int, FilterHandler>>(
			  StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Constructor for deserializing of the object
		/// </summary>
		protected Header(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		/// Get header identity content.
		/// </summary>
		public new string this[string name]
		{
			get { return base[name]; }
			set { base[name] = FilterHeaderIdentity(name, DecodeHeaderFieldValue(value)); }
		}

		/// <summary>
		/// Filter header identity.
		/// </summary>
		/// <param name="name">Name of identity (header name).</param>
		/// <param name="value">Content of identity.</param>
		/// <returns></returns>
		public string FilterHeaderIdentity(string name, string value)
		{
			string result = value;
			if (filters.ContainsKey(name))
				foreach (FilterHandler handler in filters[name].Values)
					result = handler(name, result);

			return result;
		}

		/// <summary>
		/// Add header identity's filter to the chain.
		/// </summary>
		/// <param name="name">Name of identity.</param>
		/// <param name="handler">Filter handler.</param>
		/// <param name="priority">Filter priority.</param>
		public void AddFilter(string name, FilterHandler handler, int priority)
		{
			if (!filters.ContainsKey(name))
				filters[name] = new SortedList<int, FilterHandler>();
			filters[name].Add(priority, handler);
		}

		/// <summary>
		/// Add header identity's filter to the chain with default priority.
		/// </summary>
		/// <param name="name">Name of identity.</param>
		/// <param name="handler">Filter handler.</param>
		public void AddFilter(string name, FilterHandler handler)
		{
			AddFilter(name, handler, 0);
		}

		/// <summary>
		/// Remoce header identity's filter from the chain.
		/// </summary>
		/// <param name="name">Name of identity.</param>
		/// <param name="handler">Filter handler.</param>
		public void RemoveFilter(string name, FilterHandler handler)
		{
			SortedList<int, FilterHandler> filterList = filters[name];
			if (filterList != null)
			{
				int position = filterList.IndexOfValue(handler);
				if (position != -1)
					filterList.RemoveAt(position);
			}
		}

		/// <summary>
		/// Regular expressions for mime header folding
		/// </summary>
		protected static readonly Regex headerFolding =
			new Regex(@"(?>(.{1," + Util.LineLength + @"})([ \t]|$))(?=[ \t]*\S)", RegexOptions.Compiled);

		/// <summary>
		/// regular expression for replacing non-ascii symbols
		/// </summary>
		protected static readonly Regex nonAsciiReplace =
			new Regex(@"\s*(?<quote>"")?\s*((?=(?(quote)[^""]*[^\x00-\x7F].*?""|\S*[^\x00-\x7F]\S*))\S+\s*)+(?(quote)"")",
				RegexOptions.Compiled);

		/// <summary>
		/// Get MIME encoded header item (if don't fit in ASCII symbols) with specific text and MIME encodings
		/// </summary>
		public string this[string name, Encoding encoding, ContentTransferEncoding mimeEncoding]
		{
			// use MIME encoding only if non-ascii symbols & quoted-printable or base64 mime encoding
			get
			{
				if (this[name] == null)
					return null;

				MatchEvaluator nonAsciiReplacer = delegate(Match match)
				{
					return mimeEncoding == ContentTransferEncoding.Unknown? match.Value:
							Util.Encode(match.Value, encoding, mimeEncoding, true, false);
				};

				return headerFolding.Replace(nonAsciiReplace.Replace(this[name], nonAsciiReplacer),
					string.Format("$1{0}$2", Util.CRLF));
			}
		}

		/// <summary>
		/// Get Quoted-Printable MIME encoded header item
		/// </summary>
		public string this[string name, Encoding encoding]
		{
			get { return this[name, encoding, ContentTransferEncoding.QoutedPrintable]; }
		}

		/// <summary>
		/// Get whole encoded header 
		/// </summary>
		public string Encode(Encoding encoding, ContentTransferEncoding mimeEncoding)
		{
			StringBuilder builder = new StringBuilder(512);
			foreach (string key in AllKeys)
			{
				builder
					.Append(key)
					.Append(": ")
					.Append(this[key, encoding, mimeEncoding])
					.Append(Util.CRLF);
			}
			return builder.ToString();
		}

		/// <summary>
		/// Get encoded header with Quoted-Printable mime encoding
		/// </summary>
		public string Encode(Encoding encoding)
		{
			return Encode(encoding, ContentTransferEncoding.QoutedPrintable);
		}

		/// <summary>
		/// Regular expression for detect encoded parts
		/// </summary>
		protected static readonly Regex extractEncodedParts =
			new Regex(@"=\?(?<charset>\S+?)\?(?<encoding>(?<q>[qQ])|(?<b>[bB]))\?" +
				@"(?<value>(?(q)(=[0-9A-Fa-f]{2}|[\x00-\x3C\x3E-\x7F])*(=[0-9A-Fa-f]{2}|[\x00-\x08\x0A-\x1F\x21-\x3C\x3E-\x7F])|[A-Za-z0-9+/=]+?))\?=",
				RegexOptions.Compiled);
		/// <summary>
		/// Regular expressions for removing non-sign spaces between encoded parts
		/// </summary>
		protected static readonly Regex spaceBetweenEncodedParts =
			new Regex(@"(?<=\?=)[ \t]+(?==\?)", RegexOptions.Compiled);
		/// <summary>
		/// Decode mime encoded parts
		/// </summary>
		/// <param name="encodedValue">Regex match with character sequence.</param>
		/// <returns></returns>
		public static string DecodeHeaderFieldValue(string encodedValue)
		{
			return (encodedValue == null) ? null :
				extractEncodedParts.Replace(spaceBetweenEncodedParts.Replace(encodedValue, ""),
					new MatchEvaluator(DecodeEncodedMatch));
		}

		/// <summary>
		/// Decode MIME-Encoded header parts.
		/// </summary>
		/// <param name="encodedMatch">Regex match with encoded part.</param>
		/// <returns></returns>
		protected static string DecodeEncodedMatch(Match encodedMatch)
		{
			string result = encodedMatch.Groups["value"].Value;
			switch (encodedMatch.Groups["encoding"].Value.ToUpper())
			{
				//quoted-printable
				case "Q":
					// Underscore in Q-encoded header means space. See RFC #...
					result = result.Replace("_", " ");
					return Encoding.GetEncoding(encodedMatch.Groups["charset"].Value).GetString(
						Util.FromQuotedPrintableString(result));
				// base64
				case "B":
					return Encoding.GetEncoding(encodedMatch.Groups["charset"].Value).GetString(
						Convert.FromBase64String(result));
			}
			return result;
		}

		/// <summary>
		/// Get MIME formatted presentation of the header.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			// encode header without MIME encoding
			return Encode(Encoding.Unicode, ContentTransferEncoding.Unknown);
		}

		/// <summary>
		/// Regular expression for unfolding header bodies
		/// </summary>
		static readonly protected Regex unfoldHeaderField =
			new Regex(Util.CRLF + @"(?=[ \t])", RegexOptions.Compiled);
		/// <summary>
		/// Unfold header
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string Unfold(string text)
		{
			return unfoldHeaderField.Replace(text, "");
		}
	}
}