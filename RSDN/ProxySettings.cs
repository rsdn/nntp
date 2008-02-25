using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

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
					var result = new MemoryStream();
					using (var encryptor = cryptoAlgotithm.CreateEncryptor())
						using (var cryptoStream = new CryptoStream(result, encryptor, CryptoStreamMode.Write))
						{
							var source = Encoding.UTF8.GetBytes(uriBuilder.Password);
							cryptoStream.Write(source, 0, source.Length);
							cryptoStream.FlushFinalBlock();
						}
					return result.ToArray();
				}
				return null;
			}
			set
			{
				if ((value != null) && (value.Length > 0) && (uriBuilder != null))
				{
					SymmetricAlgorithm cryptoAlgotithm = new RijndaelManaged();
					SetKeys(cryptoAlgotithm);
					var source = new MemoryStream(value);
					var result = new byte[value.Length];
					using (var decryptor = cryptoAlgotithm.CreateDecryptor())
						using (var cryptoStream = new CryptoStream(source, decryptor, CryptoStreamMode.Read))
						{
							var count = cryptoStream.Read(result, 0, result.Length);
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
			var Key = new byte[cryptoAlgotithm.Key.Length];
			var addressBytes = Encoding.UTF8.GetBytes(Address);
			Array.Copy(addressBytes, 0, Key, 0, Math.Min(Key.Length, addressBytes.Length));
			cryptoAlgotithm.Key = Key;

			var IV = new byte[cryptoAlgotithm.IV.Length];
			var usernameBytes = Encoding.UTF8.GetBytes(Username);
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