using System;
using System.Collections.Generic;

namespace Utils
{
	public sealed class Logger
	{
		private readonly Action<string> printDelegate;
		private readonly Queue<string> logEntries;
		private readonly object lockObject;

		public Logger(Action<string> printDelegate)
		{
			this.printDelegate = printDelegate;

			logEntries = new Queue<string>();
			lockObject = new object();
		}

		public void Log(Exception exception) => Log($"Exception: '{exception.Message}'");

		public void Log(string message)
		{
			lock(lockObject)
			{
				logEntries.Enqueue(message);
			}	
		}

		public void Print()
		{
			lock(lockObject)
			{
				while(logEntries.Count > 0)
					printDelegate?.Invoke(logEntries.Dequeue());
			}
		}
	}
}