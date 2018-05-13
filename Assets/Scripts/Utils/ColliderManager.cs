using System.Collections.Generic;
using System.Threading;
using EntityID = System.UInt16;

namespace Utils
{
	public class ColliderManager
	{
		private struct TestData
		{
			public Line TestLine;
			public AABox TestBounds;
			public Ray TestRay;

			public TestData(Line testLine)
			{
				TestLine = testLine;
				TestBounds = testLine.GetBounds();
				TestRay = testLine.GetRay();
			}
		}

		private struct Node
		{
			public AABox volume;
			public List<Node> children;
			public List<Entry> entries;
			private object entriesLock;

			public Node(AABox volume)
			{
				this.volume = volume;
				children = new List<Node>();
				entries = new List<Entry>();
				entriesLock = new object();
			}

			public void Subdivide()
			{
				if(children.Count == 0)
				{
					AABox[] subBoxes = new AABox[8];
					AABox.Subdivide(volume, subBoxes);
					for (int i = 0; i < 8; i++)
					{
						Node subNode = new Node(subBoxes[i]);
						children.Add(subNode);
					}
				}
				else
				{
					for (int i = 0; i < children.Count; i++)
						children[i].Subdivide();
				}
			}

			public void Add(Entry entry)
			{
				if(AABox.Intersect(volume, entry.Box))
				{
					if(children.Count > 0)
					{
						for (int i = 0; i < children.Count; i++)
							children[i].Add(entry);
					}
					else
					{
						lock(entriesLock)
						{
							entries.Add(entry);
						}
					}
				}
			}

			public bool Intersect(TestData intersectData, out EntityID entity)
			{
				if(AABox.Intersect(volume, intersectData.TestBounds))
				{
					for (int i = 0; i < children.Count; i++)
						if(children[i].Intersect(intersectData, out entity))
							return true;

					for (int i = 0; i < entries.Count; i++)
						if(entries[i].Intersect(intersectData, out entity))
							return true;
				}
				entity = 0;
				return false;
			}

			public void ClearEntries()
			{
				for (int i = 0; i < children.Count; i++)
					children[i].ClearEntries();
				entries.Clear();
			}
		}

		private struct Entry
		{
			public AABox Box;
			public EntityID Entity;

			public Entry(AABox box, EntityID entity)
			{
				Box = box;
				Entity = entity;
			}

			public bool Intersect(TestData intersectData, out EntityID entity)
			{
				entity = Entity;

				//First test if the bounds intersect
				if(!AABox.Intersect(Box, intersectData.TestBounds))
					return false;

				//Then test if the ray intersects
				float rayTime;
				if(!AABox.Intersect(Box, intersectData.TestRay, out rayTime))
					return false;

				//Then test if that ray was still within the line
				return intersectData.TestLine.SqrMagnitude <= (rayTime * rayTime);
			}
		}

		private readonly Node root;

		public ColliderManager(AABox area, int depth = 5)
		{
			root = new Node(area);
			for (int i = 0; i < depth; i++)
				root.Subdivide();
		}

		public void Add(AABox box, EntityID entity)
		{
			root.Add(new Entry(box, entity));
		}

		public bool Intersect(Line line, out EntityID target)
		{
			TestData testData = new TestData(line);
			return root.Intersect(testData, out target);
		}

		public void Clear()
		{
			root.ClearEntries();
		}
	}
}