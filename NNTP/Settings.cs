using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Collections;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for NNTPSettings.
	/// </summary>
	public class NNTPSettings
	{
		public NNTPSettings()
		{
			bindings = new ServerEndPoint[0];
		}

		public NNTPSettings(NNTPSettings settings)
		{
			bindings = settings.bindings;
			dataProviderType = settings.dataProviderType;
			DataProviderSettings = settings.DataProviderSettings;
			name = settings.name;
		}

		protected string name = Guid.NewGuid().ToString();
		public string Name
		{
			get { return name; }
			set
			{
				if (value == null || value == "")
					throw new ArgumentException("Name can't be empty");
				name = value;
			}
		}

		protected Type dataProviderType = typeof(object);
		protected Type dataProviderSettingsType = typeof(object);
		[XmlIgnore]
		[Browsable(false)]
		public Type DataProviderType
		{
			get { return dataProviderType; }
			set
			{
				if (value.GetInterface(typeof(IDataProvider).FullName) == null)
					throw new ArgumentException("DataProviderType must realize IDataProvider interface.");
				dataProviderType = value;
				dataProviderSettingsType = ((IDataProvider)Activator.CreateInstance(dataProviderType)).GetConfigType();
				if (!dataProviderType.IsInstanceOfType(dataProviderSettings))
					dataProviderSettings = Activator.CreateInstance(dataProviderSettingsType);
			}
		}

		[Browsable(false)]
		public string DataProviderTypeName
		{
			get { return DataProviderType.AssemblyQualifiedName; }
			set { DataProviderType = Type.GetType(value, true); }
		}

		protected object dataProviderSettings;
		[Browsable(false)]
		public object DataProviderSettings
		{
			get { return dataProviderSettings; }
			set
			{
				if (!dataProviderSettingsType.IsInstanceOfType(value))
					throw new ArgumentException("DataProviderSettings is not instance of " + dataProviderType.ToString() +
						" class.");
				dataProviderSettings = value;
			}
		}

		[XmlAnyElement()]
		public XmlElement RawSettings;

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
			XmlTextWriter fileWriter = new XmlTextWriter(stream, System.Text.Encoding.UTF8);
			fileWriter.Formatting = Formatting.Indented;
			XmlSerializer serializer = new XmlSerializer(this.GetType(), null,
				new Type[]{(DataProviderSettings != null) ? DataProviderSettings.GetType() : typeof(object)},
				new XmlRootAttribute("Settings"), null);
			serializer.Serialize(fileWriter, this);
			fileWriter.Close();
		}

		public static NNTPSettings Deseriazlize(string filename)
		{
			XmlReader fileReader = new XmlTextReader(filename);

			ArrayList dataProviderTypes = new ArrayList();
			// collect all data provider's types
			while (fileReader.Read())
			{
				if (fileReader.NodeType == XmlNodeType.Element && fileReader.Name == "DataProviderTypeName")
					dataProviderTypes.Add(((IDataProvider)Activator.CreateInstance(
						Type.GetType(fileReader.ReadString(), true))).GetConfigType());
			}
			fileReader.Close();

			XmlSerializer serializer = new XmlSerializer(typeof(NNTPSettings), null,
				(Type[])dataProviderTypes.ToArray(typeof(Type)), new XmlRootAttribute("Settings"), null);
			
			fileReader = new XmlTextReader(filename);

			NNTPSettings serverSettings = (NNTPSettings)serializer.Deserialize(fileReader);

			fileReader.Close();

			return serverSettings;
		}
	}	
}