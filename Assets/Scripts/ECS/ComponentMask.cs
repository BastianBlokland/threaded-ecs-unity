using CompID = System.Byte;

namespace ECS
{
    public struct ComponentMask
    {
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

		public void Set(CompID comp)
		{
			Set(new ComponentMask(comp));
		}

		public void Set(ComponentMask other)
		{
			val1 |= other.val1;
			val2 |= other.val2;
			val3 |= other.val3;
			val4 |= other.val4;
		}

		public void Unset(CompID comp)
		{
			Unset(new ComponentMask(comp));
		}

		public void Unset(ComponentMask other)
		{
			val1 &= ~other.val1;
			val2 &= ~other.val2;
			val3 &= ~other.val3;
			val4 &= ~other.val4;
		}

		public bool Has(CompID comp)
		{
			return Has(new ComponentMask(comp));
		}

		public bool Has(ComponentMask other)
		{
			return	(other.val1 & val1) == other.val1 &&
					(other.val2 & val2) == other.val2 &&
					(other.val3 & val3) == other.val3 &&
					(other.val4 & val4) == other.val4;
		}

		public void Clear()
		{
			val1 = 0;
			val2 = 0;
			val3 = 0;
			val4 = 0;
		}
    }
}