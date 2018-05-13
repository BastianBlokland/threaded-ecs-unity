using System.Collections.Generic;
using System.Threading;
using EntityID = System.UInt16;

namespace Utils
{
	public class ColliderManager
	{
		private struct LineTestData
		{
			public Line TestLine;
			public AABox TestBounds;
			public Ray TestRay;

			public LineTestData(Line testLine)
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
				if(entry.Intersect(volume))
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

			public bool Intersect(AABox box, out EntityID entity)
			{
				if(AABox.Intersect(volume, box))
				{
					for (int i = 0; i < children.Count; i++)
						if(children[i].Intersect(box, out entity))
							return true;

					for (int i = 0; i < entries.Count; i++)
						if(entries[i].Intersect(box, out entity))
							return true;
				}
				entity = 0;
				return false;
			}

			public bool Intersect(LineTestData intersectData, out EntityID entity)
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
			private readonly AABox box;
			private readonly EntityID entity;

			public Entry(AABox box, EntityID entity)
			{
				this.box = box;
				this.entity = entity;
			}

			public bool Intersect(AABox box) => AABox.Intersect(this.box, box);

			public bool Intersect(AABox box, out EntityID entity)
			{
				entity = this.entity;
				return AABox.Intersect(this.box, box);
			}

			public bool Intersect(LineTestData intersectData, out EntityID entity)
			{
				entity = this.entity;

				//First test if the bounds intersect
				if(!AABox.Intersect(box, intersectData.TestBounds))
					return false;

				//Then test if the ray intersects
				float rayTime;
				if(!AABox.Intersect(box, intersectData.TestRay, out rayTime))
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

		public bool Intersect(AABox box, out EntityID entity)
		{
			return root.Intersect(box, out entity);
		}

		public bool Intersect(Line line, out EntityID entity)
		{
			LineTestData testData = new LineTestData(line);
			return root.Intersect(testData, out entity);
		}

		public void Clear()
		{
			root.ClearEntries();
		}
	}
}