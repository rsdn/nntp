using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.IO;
using derIgel.NNTP;
using System.Runtime.CompilerServices;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// Summary description for Cache.
	/// </summary>
	[Serializable]
	public class Cache
	{
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
		/// get message by message-id
		/// </summary>
		public NewsArticle this[string messageID]
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get {return cache[messageID] as NewsArticle; }
		}

		/// <summary>
		/// get message by number in specified news group
		/// </summary>
		public NewsArticle this[string newsGroup, int number]
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				if (identities[newsGroup] != null)
				{
					string messageID = ((Hashtable)identities[newsGroup])[number] as string;
					return (messageID == null) ? null : this[messageID];
				}
				else
					return null;
			}
		}

		/// <summary>
		/// set message by message-id and number in specified newsgroup
		/// </summary>
		public NewsArticle this[string messageID, string newsGroup, int number]
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			set
			{
				if (cache.Count >= capacity)
					// cache is full, delete the oldest message
					RemoveOldestMessage();

				if (identities[newsGroup] == null)
					identities[newsGroup] = new Hashtable();

				((Hashtable)identities[newsGroup])[number] = messageID;

				
				if (!queue.Contains(messageID))
					// new message
					queue.Enqueue(messageID);
				else
				{
					Queue tempQueue = new Queue();
					foreach (string message in queue)
						if (message != messageID)
							tempQueue.Enqueue(message);
					tempQueue.Enqueue(messageID);

					queue = tempQueue;
				}

				cache[messageID] = value;
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
