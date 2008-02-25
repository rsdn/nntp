using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Cache of messages' references
	/// </summary>
	[Serializable]
	public class ReferenceCache : ISerializable
	{
		readonly Dictionary<int, int> identityTree = new Dictionary<int, int>();
		readonly IDictionary<int, int[]> identityList = new Dictionary<int, int[]>();

		/// <summary>
		/// Cache, to store message's references.
		/// </summary>
		public ReferenceCache()
		{
		}

		/// <summary>
		/// Add message's reference to cache.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="parentId"></param>
		public void AddReference(int id, int parentId)
		{
			// add to tree
			identityTree[id] = parentId;
			// build linear list
			BuildIdentityList(id);
		}

		/// <summary>
		/// Remove message's references from cache.
		/// </summary>
		/// <param name="id"></param>
		public void RemoveReference(int id)
		{
			// don't have this identity
			if (!identityTree.ContainsKey(id))
				return;

			// get parent of removing element
			var parentId = identityTree[id];
			// remove element
			identityTree.Remove(id);
			identityList.Remove(id);
			// change parent of child elements of removed element
			IList<int> changedElements = new List<int>();
			if (identityTree.ContainsValue(id))
				foreach (var key in identityTree.Keys)
					if (identityTree[key] == id)
					{
						identityTree[key] = parentId;
						changedElements.Add(key);
					}
			// rebuild corresponding linear lists
			foreach (var changedIdentity in changedElements)
				BuildIdentityList(changedIdentity);
		}

		/// <summary>
		/// Get message's references.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int[] GetReferences(int id)
		{
			return (identityList.ContainsKey(id)) ?	identityList[id] : new int[0];
		}

		/// <summary>
		/// Build/Rebuild message's references tree.
		/// </summary>
		/// <param name="id"></param>
		protected void BuildIdentityList(int id)
		{
			var identities = new List<int>();
			var traverser = id;
			while (traverser != 0)
			{
				identities.Add(traverser);
				traverser = identityTree.ContainsKey(traverser) ? identityTree[traverser] : 0;
			}
			identityList[id] = identities.ToArray();
		}

		/// <summary>
		/// Deserialzation
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public ReferenceCache(SerializationInfo info, StreamingContext context)
		{
			var identityTreeKeys = (int[])info.GetValue("identityTreeKeys", typeof(int[]));
			var identityTreeValues = (int[])info.GetValue("identityTreeValues", typeof(int[]));
			// restore tree
			for (var i = 0; i < identityTreeKeys.Length; i++)
				identityTree.Add(identityTreeKeys[i], identityTreeValues[i]);
			// rebuild linear lists
			for (var i = 0; i < identityTreeKeys.Length; i++)
				BuildIdentityList(i);
		}

		/// <summary>
		/// Serialization
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			var identityTreeKeys = new int[identityTree.Keys.Count];
			identityTree.Keys.CopyTo(identityTreeKeys, 0);
			info.AddValue("identityTreeKeys", identityTreeKeys, typeof(int[]));
		
			var identityTreeValues = new int[identityTree.Values.Count];
			identityTree.Values.CopyTo(identityTreeValues, 0);
			info.AddValue("identityTreeValues", identityTreeValues, typeof(int[]));
		}
	}
}
