using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for NNTPSettings.
	/// </summary>
	public class NNTPSettings
	{
		protected string errorOutputFilename;
		[BrowsableAttribute(false)]
		public string ErrorOutputFilename
		{
			get {return errorOutputFilename; }
			set {errorOutputFilename = value; }
		}

		public NNTPSettings()
		{
			bindings = new ServerEndPoint[0];
		}

		public NNTPSettings(NNTPSettings settings)
		{
			bindings = settings.bindings;
			dataProviderType = settings.dataProviderType;
			DataProviderSettings = settings.DataProviderSettings;
			errorOutputFilename = settings.errorOutputFilename;
			name = settings.name;
		}

		protected string name = "";
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		protected Type dataProviderType;
		[XmlIgnore]
		[Browsable(false)]
		public Type DataProviderType
		{
			get { return dataProviderType; }
			set
			{
				if (value.GetInterface(typeof(IDataProvider).FullName) == null)
					throw new Exception("DataProviderType must realize IDataProvider interface");
				dataProviderType = value;
//				DataProviderSettings = Activator.CreateInstance(
//					((IDataProvider)Activator.CreateInstance(dataProviderType)).GetConfigType());
			}
		}

		[Browsable(false)]
		public string DataProviderTypeName
		{
			get { return (DataProviderType != null) ? DataProviderType.AssemblyQualifiedName : ""; }
			set { DataProviderType = Type.GetType(value, true); }
		}

		public object DataProviderSettings;

		protected ServerEndPoint[] bindings;
		public ServerEndPoint[] Bindings
		{
			get { return bindings; }
			set { bindings = value; }
		}

		public void Serialize(string filename)
		{
			Serialize(new FileStream(filename, FileMode.Create));
		}

		public void Serialize(Stream stream)
		{
			XmlWriter fileWriter = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
			XmlSerializer serializer = new XmlSerializer(this.GetType(), null,
				new Type[]{(DataProviderSettings != null) ? DataProviderSettings.GetType() : typeof(object)},
				new XmlRootAttribute("Settings"), null);
			serializer.Serialize(fileWriter, this);
			fileWriter.Close();
		}

		public static NNTPSettings Deseriazlize(string filename)
		{
			return Deseriazlize(new FileStream(filename, FileMode.Open, FileAccess.Read));
		}

		public static NNTPSettings Deseriazlize(Stream stream)
		{
			XmlReader fileReader = new XmlTextReader(stream);
			XmlSerializer serializer = new XmlSerializer(typeof(NNTPSettings), new XmlRootAttribute("Settings"));

			NNTPSettings serverSettings = (NNTPSettings)serializer.Deserialize(fileReader);

			fileReader.Close();

			if (serverSettings.DataProviderSettings is System.Xml.XmlNode[])
			{
				XmlDocument doc = new XmlDocument();
				XmlNode settingsNode = doc.AppendChild(doc.CreateElement("DataProviderSettings"));
				foreach (XmlNode node in (System.Xml.XmlNode[])serverSettings.DataProviderSettings)
				{
					XmlNode importedNode = doc.ImportNode(node, true);
					if (importedNode is XmlElement)
						settingsNode.AppendChild(importedNode);
					else
						if (importedNode is XmlAttribute)
						settingsNode.Attributes.Append((XmlAttribute)importedNode);
				}
				Type type = ((IDataProvider)Activator.CreateInstance(serverSettings.dataProviderType)).GetConfigType();
				XmlSerializer settingsSerializer = new XmlSerializer(type, new XmlRootAttribute("DataProviderSettings"));
				XmlNodeReader xmlReader = new XmlNodeReader(settingsNode);
				serverSettings.DataProviderSettings = settingsSerializer.Deserialize(xmlReader);
				xmlReader.Close();
			}

			return serverSettings;
		}
	}	
}