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

		/// <summary>
		/// MIME encoding for non-ascii headers
		/// Only base64 & Quoted-Printable encodings have meaning
		/// </summary>
		protected ContentTransferEncoding mimeEncoding = ContentTransferEncoding.QoutedPrintable;

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

		/// <summary>
		/// get MIME encoded header item (if don't fit in ASCII symbols ) with specific encoding
		/// </summary>
		public string this[string name, Encoding encoding]
		{
			get
			{
				return (this[name] == null) ? null : 
					Util.OnlyASCIISymbols(this[name]) ? this[name] : Util.Encode(this[name], encoding, mimeEncoding, true, false);
			}
		}

		public string Encoded(Encoding encoding)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string key in AllKeys)
				builder.AppendFormat("{0}: {1}{2}", key, this[key, encoding], Util.CRLF);
			return builder.ToString();
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
	}
}
