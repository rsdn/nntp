using System;
using System.Collections.Specialized;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace derIgel.MIME
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
		/// hashtable for header identities filters
		/// </summary>
		protected Hashtable filters;

		public Header() : base()
		{
			filters = CollectionsUtil.CreateCaseInsensitiveHashtable();
		}

		public new string this[string name]
		{
			get	{return base[name]; }
			set {base[name] = FilterHeaderIdentity(name, DecodeHeaderFieldValue(value));}
		}

		public string FilterHeaderIdentity(string name, string value)
		{
			string result = value;
			if (filters[name] != null)
				foreach (FilterHandler handler in (IEnumerable)filters[name])
					result = handler(name, result);

			return result;
		}

		public void AddFilter(string name, FilterHandler handler)
		{
			if (filters[name] == null)
				filters[name] = new ArrayList();
			((ArrayList)filters[name]).Add(handler);
		}

		public void RemoveFilter(string name, FilterHandler handler)
		{
			if (filters[name] != null)
				((ArrayList)filters[name]).Remove(handler);
		}

		// text & mime encoding to pass in non-ascii replacer
		protected Encoding encoding;
		protected ContentTransferEncoding mimeEncoding;

		static protected readonly Regex nonAsciiReplace = new Regex(@"([^\x00-\xFF]+\s*)+(?<!\s)", RegexOptions.Compiled);
		/// <summary>
		/// Get MIME encoded header item (if don't fit in ASCII symbols) with specific text & MIME encodings
		/// </summary>
		public string this[string name, Encoding encoding, ContentTransferEncoding mimeEncoding]
		{
			// use MIME encoding only if non-ascii symbols & quoted-printable or base64 mime encoding
			get
			{
				if (this[name] == null)
					return null;
				
				this.encoding = encoding;
				this.mimeEncoding = mimeEncoding;

				return nonAsciiReplace.Replace(this[name], new MatchEvaluator(NonAsciiReplacer));
			}
		}

		protected string NonAsciiReplacer(Match match)
		{
			return Util.Encode(match.Value, encoding, mimeEncoding, true, false);
		}

		/// <summary>
		/// Get Quoted-Printable MIME encoded header item
		/// </summary>
		public string this[string name, Encoding encoding]
		{
			get	{ return this[name, encoding, ContentTransferEncoding.QoutedPrintable]; }
		}

		/// <summary>
		/// Get whole encoded header 
		/// </summary>
		public string Encode(Encoding encoding, ContentTransferEncoding mimeEncoding)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string key in AllKeys)
				builder.AppendFormat("{0}: {1}{2}", key, this[key, encoding, mimeEncoding], Util.CRLF);
			return builder.ToString();
		}

		/// <summary>
		/// Get encoded header with Quoted-Printable mime encoding
		/// </summary>
		public string Encode(Encoding encoding)
		{
			return Encode(encoding, ContentTransferEncoding.QoutedPrintable);
		}
			
		static readonly protected Regex extractEncodedParts =
			new Regex(@"=\?(?<charset>\S+?)\?(?<encoding>[qQbB])\?(?<value>[^\?\s]+?)\?=");

		public static string DecodeHeaderFieldValue(string encodedValue)
		{
			return (encodedValue == null) ? null :
				extractEncodedParts.Replace(encodedValue, new MatchEvaluator(DecodeEncodedMatch));
		}

		protected static string DecodeEncodedMatch(Match encodedMatch)
		{
			string result = encodedMatch.Groups["value"].Value;
			switch (encodedMatch.Groups["encoding"].Value.ToUpper())
			{
				//quoted-printable
				case "Q" :
					return Encoding.GetEncoding(encodedMatch.Groups["charset"].Value).GetString(
						Util.FromQuotedPrintableString(result));
					// base64
				case "B" :
					return Encoding.GetEncoding(encodedMatch.Groups["charset"].Value).GetString(
						Convert.FromBase64String(result));
			}
			return result;
		}

		public override string ToString()
		{
			// encode header without MIME encoding
			return Encode(Encoding.Unicode, ContentTransferEncoding.Unknown);
		}
	}
}
