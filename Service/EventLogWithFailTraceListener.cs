using System;
using System.Diagnostics;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Trace listener to write to event log. Especially process Fail method.
	/// </summary>
	public class EventLogWithFailTraceListener : TraceListener
	{
		// underlying event log
		EventLog eventLog;

		public EventLogWithFailTraceListener()
		{
			NeedIndent = false;
		}

		public EventLogWithFailTraceListener(EventLog eventLog) : base(eventLog.Source)
		{
			this.eventLog = eventLog;
			NeedIndent = false;
		}

		public EventLogWithFailTraceListener(string source) : base(source)
		{
			eventLog = new EventLog();
			eventLog.Source = source;
			NeedIndent = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				this.Close();
		}

		public override void Close()
		{
			if (eventLog != null)
				eventLog.Close();
		}

		public override void Write(string message)
		{
			if (eventLog != null)
				eventLog.WriteEntry(message);
		}

		public override void WriteLine(string message)
		{
			Write(message);
		}

		public override void Fail(string message, string detailMessage)
		{
			if (eventLog != null)
				eventLog.WriteEntry(message + " " + detailMessage, EventLogEntryType.Error);
		}

	}
}
