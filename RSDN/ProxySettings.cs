using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Serialization;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Rsdn.RsdnNntp.Public
{
	/// <summary>
	/// Helper class for XML Serializatiob of WebProxy class
	/// </summary>
	public class ProxySettings
	{
		protected UriBuilder uriBuilder;
		public ProxySettings()	{	}

		public ProxySettings(WebProxy proxy)
		{
			if ((proxy != null) && (proxy.Address != null))
			{
				uriBuilder = new UriBuilder(proxy.Address);
				if ((proxy.Credentials as NetworkCredential) != null)
				{
					uriBuilder.UserName = ((NetworkCredential)proxy.Credentials).UserName;
					uriBuilder.Password = ((NetworkCredential)proxy.Credentials).Password;
				}
			}
		}

		public string Address
		{
			get { return (uriBuilder == null) ? null : uriBuilder.Scheme + Uri.SchemeDelimiter + uriBuilder.Uri.Authority; }
			set { uriBuilder = new UriBuilder(value); }
		}

		public string Username
		{
			get { return (uriBuilder == null) ? null : uriBuilder.UserName; }
			set { uriBuilder.UserName = value; }
		}

		/// <summary>
		/// encrypted password
		/// </summary>
		[XmlElement(DataType = "base64Binary")]
		public byte[] Password
		{
			get
			{
				if ((uriBuilder != null) && (uriBuilder.Password != ""))
				{
					SymmetricAlgorithm cryptoAlgotithm = new RijndaelManaged();
					SetKeys(cryptoAlgotithm);
					MemoryStream result = new MemoryStream();
					using (ICryptoTransform encryptor = cryptoAlgotithm.CreateEncryptor())
						using (CryptoStream cryptoStream = new CryptoStream(result, encryptor, CryptoStreamMode.Write))
						{
							byte[] source = Encoding.UTF8.GetBytes(uriBuilder.Password);
							cryptoStream.Write(source, 0, source.Length);
							cryptoStream.FlushFinalBlock();
						}
					return result.ToArray();
				}
				else
					return null;
			}
			set
			{
				if ((value != null) && (value.Length > 0) && (uriBuilder != null))
				{
					SymmetricAlgorithm cryptoAlgotithm = new RijndaelManaged();
					SetKeys(cryptoAlgotithm);
					MemoryStream source = new MemoryStream(value);
					byte[] result = new byte[value.Length];
					using (ICryptoTransform decryptor = cryptoAlgotithm.CreateDecryptor())
						using (CryptoStream cryptoStream = new CryptoStream(source, decryptor, CryptoStreamMode.Read))
						{
							int count = cryptoStream.Read(result, 0, result.Length);
							uriBuilder.Password = Encoding.UTF8.GetString(result, 0, count);
						}
				}

			}
		}

		/// <summary>
		/// Genereate keys for encryption/decryption
		/// </summary>
		/// <param name="cryptoAlgotithm"></param>
		private void SetKeys(SymmetricAlgorithm cryptoAlgotithm)
		{
			byte[] Key = new byte[cryptoAlgotithm.Key.Length];
			byte[] addressBytes = Encoding.UTF8.GetBytes(Address);
			Array.Copy(addressBytes, 0, Key, 0, Math.Min(Key.Length, addressBytes.Length));
			cryptoAlgotithm.Key = Key;

			byte[] IV = new byte[cryptoAlgotithm.IV.Length];
			byte[] usernameBytes = Encoding.UTF8.GetBytes(Username);
			Array.Copy(usernameBytes, 0, IV, 0, Math.Min(IV.Length, usernameBytes.Length));
			cryptoAlgotithm.IV = IV;
		}

		[XmlIgnore]
		public WebProxy Proxy
		{
			get
			{
				return (uriBuilder == null) ? new WebProxy() :
					new WebProxy(uriBuilder.Uri, false, null,	new NetworkCredential(uriBuilder.UserName, uriBuilder.Password));
			}		
		}
	}
}