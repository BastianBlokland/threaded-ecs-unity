using TagID = System.Byte;

namespace ECS.Storage
{
    public struct TagMask
    {
		public static TagMask Empty { get; } = new TagMask();
		
		public static TagMask Full { get; } = Empty.Invert();

		public bool IsEmpty => val == 0;

        private long val;

		public TagMask(TagID tag)
		{
			val = 1L << tag;
		}

		public bool Has(TagMask other) => (other.val & val) == other.val;

		public bool NotHas(TagMask other) => (other.val & val) == 0;

		public TagMask Add(TagMask other)
		{
			val |= other.val;
			return this;
		}

		public TagMask Remove(TagMask other)
		{
			val &= ~other.val;
			return this;
		}

		public TagMask Invert()
		{
			val = ~val;
			return this;
		}

		public TagMask Clear()
		{ 
			val = 0; 
			return this; 
		}

		public static TagMask operator +(TagMask mask1, TagMask mask2) => mask1.Add(mask2);

		public static TagMask operator -(TagMask mask1, TagMask mask2) => mask1.Remove(mask2);
    }
}