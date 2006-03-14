using System;
using Rsdn.RsdnNntp.Public.RsdnService;

namespace Rsdn.RsdnNntp.Public
{
	/// <summary>
	/// Summary description for Group.
	/// </summary>
	public class Group : IGroup
	{
		group group;
		public Group(group group)
		{
			this.group = group;
		}

		public DateTime Created
		{
			get { return group.created; }
		}
		public string Name
		{
			get { return group.name; }
		}
		public int FirstArticleNumber
		{
			get { return group.first; }
		}
		public int LastArticleNumber
		{
			get { return group.last; }
		}
	}
}
