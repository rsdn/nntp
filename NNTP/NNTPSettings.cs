using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;

namespace derIgel
{
	namespace NNTP
	{
	/// <summary>
	/// Summary description for NNTPSettings.
	/// </summary>
		[XmlRoot("Settings")]
		public class NNTPSettings
		{
			public NNTPSettings()
			{
				bindingAddresses = IPAddress.Any;
				bindingPort = defaultPort;
			}

			/// <summary>
			/// default NNTP port
			/// </summary>
			protected const int defaultPort = 119;

			/// <summary>
			/// addresses for binding
			/// </summary>
			protected IPAddress bindingAddresses;

			[BrowsableAttribute(false)]
			public string Bindings
			{
				get
				{
					return bindingAddresses.ToString();
				}
				set
				{
					bindingAddresses = IPAddress.Parse(value);
				}
			}

			protected ushort bindingPort;

			[Category("Connections")]
			[DefaultValue(defaultPort)]
			[Description("RSDN NNTP Server port")]
			public ushort Port
			{
				get
				{
					return bindingPort;
				}
				set
				{
					bindingPort = value;
				}
			}

			[BrowsableAttribute(false)]
			[XmlIgnore]
			public IPEndPoint EndPoint
			{
				get
				{
					return new	IPEndPoint(bindingAddresses, bindingPort);
				}
			}
			public void Serialize(string filename)
			{
				Serialize(new FileStream(filename, FileMode.Create));
			}

			public void Serialize(Stream stream)
			{
				XmlWriter fileWriter = new XmlTextWriter(stream, System.Text.Encoding.ASCII);
				XmlSerializer serializer = new XmlSerializer(this.GetType());
				serializer.Serialize(fileWriter, this);
				fileWriter.Close();
			}

			public static object Deseriazlize(string filename, Type type)
			{
				return Deseriazlize(new FileStream(filename, FileMode.Open), type);
			}

			public static object Deseriazlize(Stream stream, Type type)
			{
				XmlReader fileReader = new XmlTextReader(stream);
				XmlSerializer serializer = new XmlSerializer(type);
				object serverSettings = serializer.Deserialize(fileReader);
				fileReader.Close();

				return serverSettings;
			}
		}	
	}
}