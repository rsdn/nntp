using System;

namespace Rsdn.Nntp.Commands
{
	/// <summary>
	/// Attribute identifies that assembly contains NNTP command classes
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class NntpCommandsAssemblyAttribute : Attribute
	{
	}
		
	/// <summary>
	/// Attribute for NNTP command classes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public class NntpCommandAttribute : Attribute
	{
		public NntpCommandAttribute(string commandName)
		{
			command = commandName;
		}
		protected string command;

		internal string Name
		{
			get
			{
				return command;
			}
		}
	}
}
