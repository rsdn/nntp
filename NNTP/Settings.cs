using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Rsdn.Nntp.Editor;

namespace Rsdn.Nntp
{
	/// <summary>
	/// General settings for NNTP Virtual Server.
	/// </summary>
	public class NntpSettings
	{
		/// <summary>
		/// Create default settings.
		/// </summary>
		public NntpSettings()
		{
			bindings = new ServerEndPoint[0];
		}

		/// <summary>
		/// Create settings based on existing settings.
		/// </summary>
		/// <param name="settings">Settings to copy.</param>
		public NntpSettings(NntpSettings settings)
		{
			bindings = settings.bindings;
			dataProviderType = settings.dataProviderType;
			DataProviderSettings = settings.DataProviderSettings;
			name = settings.name;
		}

		/// <summary>
		/// Server's name
		/// </summary>
		protected string name = "";
		/// <summary>
		/// Server's name accessor
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Name can't be empty");
				name = value;
			}
		}

		/// <summary>
		/// Data Provider's type.
		/// </summary>
		protected Type dataProviderType = typeof(object);
		/// <summary>
		/// Data Provider's type accessor.
		/// </summary>
		[XmlIgnore]
		[TypeConverter(typeof(TypeClassConverter))]
		[EditorAttribute(typeof(TypeEditor), typeof(UITypeEditor))]
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
		/// <summary>
		/// Name of Data Provider's type.
		/// </summary>
		[Browsable(false)]
		public string DataProviderTypeName
		{
			get { return dataProviderType.AssemblyQualifiedName; }
			set { DataProviderType = Type.GetType(value, true); }
		}

		/// <summary>
		/// Data Provider settings' type
		/// </summary>
		protected Type dataProviderSettingsType = typeof(object);
		protected object dataProviderSettings = new object();
		[Browsable(false)]
		public object DataProviderSettings
		{
			get { return dataProviderSettings; }
			set
			{
				if (!dataProviderSettingsType.IsInstanceOfType(value))
					throw new ArgumentException("DataProviderSettings is not instance of " + dataProviderType +
						" class.");
				dataProviderSettings = value;
			}
		}

		/// <summary>
		/// Server TCP/IP endpoins.
		/// </summary>
		protected ServerEndPoint[] bindings;
		/// <summary>
		/// Server TCP/IP endpoints accessor.
		/// </summary>
		[TypeConverter(typeof(BindingsConverter))]
		public ServerEndPoint[] Bindings
		{
			get { return bindings; }
			set { bindings = value; }
		}

		/// <summary>
		/// Serialize this settings object to file.
		/// </summary>
		/// <param name="filename">Name of the file.</param>
		public void Serialize(string filename)
		{
			Serialize(new FileStream(filename, FileMode.Create));
		}

		/// <summary>
		/// Serialize this settings object.
		/// </summary>
		/// <param name="stream">Stream to serialize.</param>
		public void Serialize(Stream stream)
		{
			var fileWriter = new XmlTextWriter(stream, Encoding.UTF8)
				{ Formatting = Formatting.Indented };
			var serializer = new XmlSerializer(GetType(), null,
				new[]{(DataProviderSettings != null) ? DataProviderSettings.GetType() : typeof(object)},
				new XmlRootAttribute("Settings"), null);
			serializer.Serialize(fileWriter, this);
			fileWriter.Close();
		}

		/// <summary>
		/// Deserialize settings from a file.
		/// </summary>
		/// <param name="filename">Name of the file.</param>
		/// <returns></returns>
		public static NntpSettings Deseriazlize(string filename)
		{
			var dataProviderTypes = new List<Type>();
			
			var doc = new XmlDocument();
			doc.Load(filename);

			// Collect all data provider's types
			foreach (XmlNode dataProviderTypeNode in doc.DocumentElement.
				SelectNodes("/Settings/DataProviderTypeName"))
				dataProviderTypes.Add(((IDataProvider)Activator.CreateInstance(
					Type.GetType(dataProviderTypeNode.InnerText, true))).GetConfigType());
			
			// Deserialize settings with known types of data provider's config objects
			var serializer = new XmlSerializer(typeof(NntpSettings), null,
				dataProviderTypes.ToArray(), new XmlRootAttribute("Settings"), null);
			
			XmlReader fileReader = new XmlNodeReader(doc);

			var serverSettings = (NntpSettings)serializer.Deserialize(fileReader);

			fileReader.Close();

			return serverSettings;
		}

		/// <summary>
		/// Thread Pool's size
		/// </summary>
		[DefaultValue(25)]
		public int ThreadPoolSize
		{
			get
			{
				int workerThreads, competitionPortThreads;
				ThreadPool.GetMaxThreads(out workerThreads, out competitionPortThreads);
				return workerThreads;
			}
			set
			{
				int workerThreads, competitionPortThreads;
				ThreadPool.GetMaxThreads(out workerThreads, out competitionPortThreads);
				ThreadPool.SetMaxThreads(value, competitionPortThreads);
			}
		}

		/// <summary>
		/// Process affinity mask.
		/// </summary>
		[DefaultValue(-1)]
		public Int64 AffinityMask
		{
			get { return (long)Process.GetCurrentProcess().ProcessorAffinity; }
			set { Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)value; }
		}

		/// <summary>
		/// Process priority.
		/// </summary>
		[DefaultValue(ProcessPriorityClass.Normal)]
		public ProcessPriorityClass Priority
		{
			get { return Process.GetCurrentProcess().PriorityClass; }
			set { Process.GetCurrentProcess().PriorityClass = value; }
		}
	}
}