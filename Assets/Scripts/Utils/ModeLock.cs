using System.Threading;

namespace Utils
{
	/// <summary>
	/// Locking utility that allows multiple entries in the same 'mode' but needs to wait for that 'mode' to be 
	/// done before entering another mode. NOTE: Be very carefull that EVERY call to 'Enter' is followed by a 
	/// call to 'Exit' otherwise deadlocks will happen.
	/// </summary>
	public sealed class ModeLock
	{
		private readonly object lockObject = new object();
		private int currentMode = -1;
		private int currentEnterCount;

		public void Enter(int mode)
		{
			while(Volatile.Read(ref currentMode) != mode)
			{
				lock(lockObject)
				{
					if(Volatile.Read(ref currentEnterCount) == 0)
						currentMode = mode;
					else
						Monitor.Wait(lockObject);
				}
			}
			Interlocked.Increment(ref currentEnterCount);
		}

		public void Exit()
		{
			if(Interlocked.Decrement(ref currentEnterCount) == 0)
			{
				lock(lockObject)
				{
					Monitor.PulseAll(lockObject);
				}
			}
		}
	}
}