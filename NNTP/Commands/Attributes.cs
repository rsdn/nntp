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
	/// Attribute specify that class implement specific NNTP command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public class NntpCommandAttribute : Attribute
	{
		/// <summary>
		/// Create attribute.
		/// </summary>
		/// <param name="commandName">NNTP Command.</param>
		public NntpCommandAttribute(string commandName)
		{
			command = commandName;
		}

		/// <summary>
		/// NNTP command string.
		/// </summary>
		protected string command;

		/// <summary>
		/// NNTP command string.
		/// </summary>
		internal string Name
		{
			get
			{
				return command;
			}
		}
	}
}
