using System;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Summary description for NntpSettings.
	/// </summary>
	public class NntpSettings
	{
		public NntpSettings()
		{
			bindings = new ServerEndPoint[0];
		}

		public NntpSettings(NntpSettings settings)
		{
			bindings = settings.bindings;
			dataProviderType = settings.dataProviderType;
			DataProviderSettings = settings.DataProviderSettings;
			name = settings.name;
		}

		protected string name = "";
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
		[TypeConverter(typeof(TypeClassConverter))]
		[EditorAttribute(typeof(TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public Type DataProviderType
		{
			get { return dataProviderType; }
			set
			{
				if (!typeof(IDataProvider).IsAssignableFrom(value))
					throw new ArgumentException("DataProviderType must realize IDataProvider interface.",
						"DataProviderType");
				dataProviderType = value;
				dataProviderSettingsType = ((IDataProvider)Activator.CreateInstance(dataProviderType)).GetConfigType();
				if (!dataProviderType.IsInstanceOfType(dataProviderSettings))
					dataProviderSettings = Activator.CreateInstance(dataProviderSettingsType);
			}
		}

		[Browsable(false)]
		public string DataProviderTypeName
		{
			get { return dataProviderType.AssemblyQualifiedName; }
			set { DataProviderType = Type.GetType(value, true); }
		}

		protected object dataProviderSettings = new object();
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
		[TypeConverter(typeof(BindingsConverter))]
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

		public static NntpSettings Deseriazlize(string filename)
		{
			ArrayList dataProviderTypes = new ArrayList();
			
			XmlDocument doc = new XmlDocument();
			doc.Load(filename);

			/// Collect all data provider's types
			foreach (XmlNode dataProviderTypeNode in doc.DocumentElement.
									SelectNodes("/Settings/DataProviderTypeName"))
				dataProviderTypes.Add(((IDataProvider)Activator.CreateInstance(
					Type.GetType(dataProviderTypeNode.InnerText, true))).GetConfigType());
			
			/// Deserialize settings with known types of data provider's config objects
			XmlSerializer serializer = new XmlSerializer(typeof(NntpSettings), null,
				(Type[])dataProviderTypes.ToArray(typeof(Type)), new XmlRootAttribute("Settings"), null);
			
			XmlReader fileReader = new XmlNodeReader(doc);

			NntpSettings serverSettings = (NntpSettings)serializer.Deserialize(fileReader);

			fileReader.Close();

			return serverSettings;
		}
	}	
}