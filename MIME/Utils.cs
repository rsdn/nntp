// $Id$
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace Rsdn.Mime
{
	// helper class
	public class Util
	{
		/// <summary>
		/// line delimeter
		/// </summary>
		public const string CRLF = "\r\n";
		/// <summary>
		/// maximum length of line
		/// </summary>
		public const int LineLength = 76;

		static protected readonly Regex split998 = new Regex(@".{1,998}", RegexOptions.Compiled);
		
		/// <summary>
		/// Encode bytes to specified MIME encoding string
		/// </summary>
		/// <param name="bytes">Bytes to encode</param>
		/// <param name="contentEncoding">MIME Encoding</param>
		/// <returns>MIME encoded byte stream</returns>
		public static string Encode(byte[] bytes, ContentTransferEncoding contentEncoding, bool breakLines)
		{
			StringBuilder result = new StringBuilder();
			switch (contentEncoding)
			{
				case ContentTransferEncoding.Base64 :
					result.Append(Convert.ToBase64String(bytes));
					if (breakLines)
						// break in lines
						for (int i = LineLength; i < result.Length; i += LineLength + CRLF.Length)
							result.Insert(i, CRLF);
					break;
				case ContentTransferEncoding.QoutedPrintable :
					result.Append(ToQuotedPrintableString(bytes, breakLines));
					break;
				case ContentTransferEncoding.SevenBit :
					// TODO: cut 8th bit or not?
				case ContentTransferEncoding.EightBit :
				case ContentTransferEncoding.Binary   :
				default :
					// split per 1000 symbols (including trailing CRLF)
					//writer.Write(encoding.GetBytes(split998.Replace(body.ToString(), "$&" + Util.CRLF)));
					result.Append(BytesToString(bytes));
					break;
			}
			return result.ToString();
		}

		/// <summary>
		/// Convert text to MIME encoded string with specified text and content transfer encodings
		/// </summary>
		/// <param name="text">Text to convert</param>
		/// <param name="header">true, if convert for MIME header</param>
		/// <param name="textEncoding">Target text encoding</param>
		/// <param name="contentEncoding">Content-Transfer encoding.
		///		In header only base64 & quoted-printable encodings have meaning</param>
		/// <returns>MIME encoded text</returns>
		public static string Encode(string text, Encoding textEncoding, ContentTransferEncoding contentEncoding, bool header, bool breakLines)
		{
			StringBuilder builder = new StringBuilder();
			bool reallyHeader = false;

			if (header)
				switch (contentEncoding)
				{
					case ContentTransferEncoding.Base64 :
					case ContentTransferEncoding.QoutedPrintable :
						reallyHeader = true;
						builder.Append("=?").Append(textEncoding.HeaderName).Append('?').
							Append((contentEncoding == ContentTransferEncoding.Base64) ? 'b' : 'q').Append('?');
						break;
				}
			
			switch (contentEncoding)
			{
				case ContentTransferEncoding.Base64 :
				case ContentTransferEncoding.QoutedPrintable :
					builder.Append(Encode(textEncoding.GetBytes(text), contentEncoding, breakLines));
					break;
				case ContentTransferEncoding.SevenBit :
					// cut 8th bit
					builder.Append(Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(text)));
					break;
				default :
					builder.Append(text);
					break;
			}

			if (reallyHeader)
				builder.Append("?=");
			
			return builder.ToString();
		}

		public static bool OnlyASCIISymbols(string text)
		{
			bool result = true;
			foreach (char symbol in text)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}

		public static bool OnlyASCIISymbols(byte[] bytes)
		{
			bool result = true;
			foreach (byte symbol in bytes)
				if (symbol > 0x7f)
				{
					result = false;
					break;
				}
			return result;
		}

		static protected readonly Regex quotedPrintableEncodedSymbol =
			new Regex(@"(?i)=(?<code>[0-9a-f]{2})", RegexOptions.Compiled);

		static protected readonly Regex quotedPrintableSoftBreaks =
			new Regex(@"=" + CRLF, RegexOptions.Compiled);
		
		public static byte[] FromQuotedPrintableString(string encodedText)
		{
			string decodedSymbols = quotedPrintableSoftBreaks.Replace(
				quotedPrintableEncodedSymbol.Replace(encodedText,
					new MatchEvaluator(quotedPrintableEncodedSymbolMatchEvaluator)), "");
		
			return StringToBytes(decodedSymbols);
		}

		static protected string quotedPrintableEncodedSymbolMatchEvaluator(Match match)
		{
		 return ((char)Convert.ToInt32(match.Groups["code"].Value, 16)).ToString();
		}

		static protected readonly Regex quotedPrintableDecodedSymbol =
			new Regex(@"[^\x09\x20\x21-\x3c\x3e-\x7e]", RegexOptions.Compiled);

		/// <summary>
		/// Regular expression for breaking 'quoted-rintable' strings in lines
		/// </summary>
		static protected readonly Regex insertQuotedPrintableSoftBreaks =
			new Regex(".{1," + LineLength + "}(?<!=.?)", RegexOptions.Compiled);

		/// <summary>
		/// Encode with 'quoted-printable' encoding
		/// </summary>
		/// <param name="bytes">source bytes</param>
		/// <param name="breakLines">break in lines, if true</param>
		/// <returns>'quoted-printable' encoded string</returns>
		public static string ToQuotedPrintableString(byte[] bytes, bool breakLines)
		{
			string quotedString = quotedPrintableDecodedSymbol.Replace(BytesToString(bytes),
					new MatchEvaluator(quotedPrintableDecodedSymbolMatchEvaluator));

			return breakLines ? insertQuotedPrintableSoftBreaks.Replace(quotedString,	"$&=" + Util.CRLF) : quotedString;
		}
		/// <summary>
		/// Encode with 'quoted-printable' encoding without line-breaking
		/// </summary>
		public static string ToQuotedPrintableString(byte[] bytes)
		{
			return ToQuotedPrintableString(bytes, false);
		}

		static protected string quotedPrintableDecodedSymbolMatchEvaluator(Match match)
		{
			return "=" + ((int)match.Value[0]).ToString("X2"); // 2 hexadeximal digits
		}

		public static string ExpandException(Exception exception)
		{
			StringBuilder result = new StringBuilder();
			while (exception != null)
			{
				result.Append(exception.Message).Append(Environment.NewLine);
				exception = exception.InnerException;
			}
			return result.ToString();
		}

		/// <summary>
		/// Convert string to raw bytes
		/// </summary>
		public static byte[] StringToBytes(string text)
		{
			return Encoding.GetEncoding("iso8859-1").GetBytes(text);
		}

		/// <summary>
		/// Direct convert raw bytes to string
		/// </summary>
		public static string BytesToString(byte[] input, int length)
		{
			return Encoding.GetEncoding("iso8859-1").GetString(input, 0, length);
		}

		public static string BytesToString(byte[] input)
		{
			return BytesToString(input, input.Length);
		}
	
	}
}