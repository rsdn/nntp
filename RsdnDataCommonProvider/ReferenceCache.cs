using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Cache of messages' references
	/// </summary>
	[Serializable]
	public class ReferenceCache : ISerializable
	{
		Hashtable identityTree = new Hashtable();
		Hashtable identityList = new Hashtable();

		public ReferenceCache()
		{
		}

		public void AddReference(int id, int parentId)
		{
			// add to tree
			identityTree[id] = parentId;
			// build linear list
			BuildIdentityList(id);
		}

		public void RemoveReference(int id)
		{
			// don't have this identity
			if (!identityTree.ContainsKey(id))
				return;

			// get parent of removing element
			int parentId = (int)identityTree[id];
			// remove element
			identityTree.Remove(id);
			identityList.Remove(id);
			// change parent of child elements of removed element
			ArrayList changedElements = new ArrayList();
			if (identityTree.ContainsValue(id))
				foreach (object key in identityTree.Keys)
					if ((int)identityTree[key] == id)
					{
						identityTree[key] = parentId;
						changedElements.Add(key);
					}
			// rebuild corresponding linear lists
			foreach (int changedIdentity in changedElements)
				BuildIdentityList(changedIdentity);
		}

		public int[] GetReferences(int id)
		{
			return (identityList[id] != null) ?	(int[])identityList[id] : new int[0];
		}

		protected void BuildIdentityList(int id)
		{
			ArrayList identities = new ArrayList();
			int traverser = id;
			while (traverser != 0)
			{
				identities.Add(traverser);
				traverser = (identityTree[traverser] != null) ? (int)identityTree[traverser] : 0;
			}
			identityList[id] = identities.ToArray(typeof(int));
		}

		/// <summary>
		/// Deserialzation
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public ReferenceCache(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			int[] identityTreeKeys = (int[])info.GetValue("identityTreeKeys", typeof(int[]));
			int[] identityTreeValues = (int[])info.GetValue("identityTreeValues", typeof(int[]));
			// restore tree
			for (int i = 0; i < identityTreeKeys.Length; i++)
				identityTree.Add(identityTreeKeys[i], identityTreeValues[i]);
			// rebuild linear lists
			for (int i = 0; i < identityTreeKeys.Length; i++)
				BuildIdentityList(i);
		}

		/// <summary>
		/// Serialization
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			int[] identityTreeKeys = new int[identityTree.Keys.Count];
			identityTree.Keys.CopyTo(identityTreeKeys, 0);
			info.AddValue("identityTreeKeys", identityTreeKeys, typeof(int[]));
		
			int[] identityTreeValues = new int[identityTree.Values.Count];
			identityTree.Values.CopyTo(identityTreeValues, 0);
			info.AddValue("identityTreeValues", identityTreeValues, typeof(int[]));
		}
	}
}
