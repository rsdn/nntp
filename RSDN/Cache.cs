using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.IO;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for Cache.
	/// </summary>
	[Serializable]
	public class Cache
	{
		public class NewsArticleIdentity
		{
			public string messageID;
			public string newsGroup;
			public int number;
			public NewsArticleIdentity(string messageID, string newsGroup, int number)
			{
				this.messageID = messageID;
				this.newsGroup = newsGroup;
				this.number = number;
			}
		}

		public Cache()
		{
			queue = new Queue();
			cache = new Hashtable();
			identities = new Hashtable();
		}

		/// <summary>
		/// capacity of cache
		/// </summary>
		int capacity = 0;
		/// <summary>
		/// stores message-id's
		/// </summary>
		protected Queue queue;
		/// <summary>
		/// stores messages by message-id key
		/// </summary>
		protected Hashtable cache;
		/// <summary>
		/// stores connections messageID to group name & message number
		/// </summary>
		protected Hashtable identities;

		public int Capacity
		{
			get {return capacity;}
			set
			{
				capacity = value;
				while (queue.Count > capacity)
					RemoveOldestMessage();
			}
		}

		/// <summary>
		/// get/set message by identity
		/// </summary>
		public NewsArticle this[NewsArticleIdentity identity]
		{
			get
			{
				try
				{
					string messageID = identity.messageID;
					if (messageID == null)
						if ((identities[identity.newsGroup] != null))
							messageID = ((Hashtable)identities[identity.newsGroup])[identity.number] as string;

					return (messageID != null) ? cache[messageID] as NewsArticle : null;
				}
				catch (NullReferenceException)
				{
					return null;
				}
			}
			set
			{
				if (cache.Count >= capacity)
					// cache is full, delete the oldest message
					RemoveOldestMessage();

				if (identities[identity.newsGroup] == null)
					identities[identity.newsGroup] = new Hashtable();

				((Hashtable)identities[identity.newsGroup])[identity.number] = identity.messageID;

				
				if (!queue.Contains(identity.messageID))
					// new message
					queue.Enqueue(identity.messageID);

				cache[identity.messageID] = value;
			}
		}

		public static Cache Deserialize(string filename)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			Cache cache = (Cache) formatter.Deserialize(stream);
			stream.Close();
			return cache;
		}

		public void Serialize(string filename)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, this);
			stream.Close();
		}

		protected void RemoveOldestMessage()
		{
			cache.Remove((string)queue.Dequeue());
			//((Hashtable)identities[identity.newsGroup]).Remove(identity.number);
		}
	}
}
