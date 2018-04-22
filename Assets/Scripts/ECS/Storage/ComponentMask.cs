using CompID = System.Byte;

namespace ECS.Storage
{
    public struct ComponentMask
    {
		public static ComponentMask Empty 
		{ 
			get { return new ComponentMask(); } 
		}

        private long val1;
		private long val2;
		private long val3;
		private long val4;

		public ComponentMask(CompID comp)
		{
			val1 = comp < 64 ? 
				1L << (comp - 0) : 0;

			val2 = comp >= 64 && comp < 128 ?
				1L << (comp - 64) : 0;

			val3 = comp >= 128 && comp < 192 ?
				1L << (comp - 128) : 0;

			val4 = comp >= 192 ?
				1L << (comp - 192) : 0;
		}

		public ComponentMask Add(ComponentMask other)
		{
			val1 |= other.val1;
			val2 |= other.val2;
			val3 |= other.val3;
			val4 |= other.val4;
			return this;
		}

		public ComponentMask Remove(ComponentMask other)
		{
			val1 &= ~other.val1;
			val2 &= ~other.val2;
			val3 &= ~other.val3;
			val4 &= ~other.val4;
			return this;
		}

		public bool Has(ComponentMask other)
		{
			return	(other.val1 & val1) == other.val1 &&
					(other.val2 & val2) == other.val2 &&
					(other.val3 & val3) == other.val3 &&
					(other.val4 & val4) == other.val4;
		}

		public bool NotHas(ComponentMask other)
		{
			return	(other.val1 & val1) == 0 &&
					(other.val2 & val2) == 0 &&
					(other.val3 & val3) == 0 &&
					(other.val4 & val4) == 0;
		}

		public ComponentMask Invert()
		{
			val1 = ~val1;
			val2 = ~val2;
			val3 = ~val3;
			val4 = ~val4;
			return this;
		}

		public ComponentMask Clear()
		{
			val1 = 0;
			val2 = 0;
			val3 = 0;
			val4 = 0;
			return this;
		}

		#region Utilities for creating masks
		public static ComponentMask CreateMask(CompID comp1)
		{
			return new ComponentMask(comp1);
		}

		public static ComponentMask CreateMask(CompID comp1, CompID comp2)
		{
			return 	new ComponentMask(comp1).Add(
					new ComponentMask(comp1));
		}

		public static ComponentMask CreateMask(CompID comp1, CompID comp2, CompID comp3)
		{
			return	new ComponentMask(comp1).Add(
					new ComponentMask(comp2)).Add(
					new ComponentMask(comp3));
		}

		public static ComponentMask CreateMask(CompID comp1, CompID comp2, CompID comp3, CompID comp4)
		{
			return	new ComponentMask(comp1).Add(
					new ComponentMask(comp2)).Add(
					new ComponentMask(comp3)).Add(
					new ComponentMask(comp4));
		}
		#endregion
    }
}