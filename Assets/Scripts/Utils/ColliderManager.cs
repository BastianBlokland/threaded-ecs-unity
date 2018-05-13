using System.Threading;
using EntityID = System.UInt16;

namespace Utils
{
	public class ColliderManager
	{
		private struct Entry
		{
			public AABox Box;
			public EntityID Entity;

			public Entry(AABox box, EntityID entity)
			{
				Box = box;
				Entity = entity;
			}
		}

		private readonly ReaderWriterLockSlim readWriteLock;
		private readonly Entry[] entries;
		private int count;

		public ColliderManager(int maxEntries)
		{
			readWriteLock = new ReaderWriterLockSlim();
			entries = new Entry[maxEntries];
		}

		public void Add(AABox box, EntityID entity)
		{
			readWriteLock.EnterWriteLock();
			{
				if(count < entries.Length)
				{
					entries[count] = new Entry(box, entity);
					count++;
				}
			}
			readWriteLock.ExitWriteLock();
		}

		public bool Intersect(Line line, out EntityID target)
		{
			AABox bounds = line.GetBounds();
			Ray ray = line.GetRay();

			target = 0;
			bool result = false;
			readWriteLock.EnterReadLock();
			{
				for (int i = 0; i < count; i++)
				{
					if(AABox.Intersect(entries[i].Box, bounds))
					{
						float rayTime;
						if(AABox.Intersect(entries[i].Box, ray, out rayTime) && line.SqrMagnitude <= (rayTime * rayTime))
						{
							target = entries[i].Entity;
							result = true;
							break;
						}
					}
				}
			}
			readWriteLock.ExitReadLock();
			return result;
		}

		public void Clear()
		{
			readWriteLock.EnterWriteLock();
			{
				count = 0;
			}
			readWriteLock.ExitWriteLock();
		}
	}
}