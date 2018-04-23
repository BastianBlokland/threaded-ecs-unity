using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Profiler
{
	public class TimelineTrack
	{
		private const int MAX_ITEM_COUNT = 100;

		private readonly Stopwatch stopWatch = new Stopwatch();
		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private readonly TimelineItem[] items = new TimelineItem[MAX_ITEM_COUNT];
		private int count = 0;
		private int currentItem = -1;
		private bool started;

		public void StartTimer()
		{
			stopWatch.Start();
			started = true;
		}

		public void GetItems(List<TimelineItem> outputList)
		{
			outputList.Clear();
			threadLock.EnterReadLock();
			{
				for (int i = 0; i < count; i++)
					outputList.Add(items[i]);
			}
			threadLock.ExitReadLock();
		}

		public void LogStartWork()
		{
			threadLock.EnterWriteLock();
			{
				if(started && (count == 0 || !items[currentItem].Running))
				{
					currentItem = (currentItem + 1) % MAX_ITEM_COUNT;
					if(count < MAX_ITEM_COUNT)
						count++;
					items[currentItem] = new TimelineItem { StartTime = (float)stopWatch.Elapsed.TotalSeconds, Running = true };
				}
			}
			threadLock.ExitWriteLock();
		}

		public void LogEndWork()
		{
			threadLock.EnterWriteLock();
			{
				if(count > 0 && items[currentItem].Running)
				{
					TimelineItem current = items[currentItem];
					current.Running = false;
					current.StopTime = (float)stopWatch.Elapsed.TotalSeconds;
					items[currentItem] = current;
				}
			}
			threadLock.ExitWriteLock();
		}
	}
}