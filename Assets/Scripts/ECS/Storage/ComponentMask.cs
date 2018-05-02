using CompID = System.Byte;

namespace ECS.Storage
{
    public struct ComponentMask
    {
		public static ComponentMask Empty 
		{ 
			get { return new ComponentMask(); } 
		}

		public static ComponentMask Full
		{
			get { return new ComponentMask().Invert(); }
		}

		public bool IsEmpty { get { return val == 0; } }

        private long val;

		public ComponentMask(CompID comp)
		{
			val = 1L << comp;
		}

		public ComponentMask Add(ComponentMask other)
		{
			val |= other.val;
			return this;
		}

		public ComponentMask Remove(ComponentMask other)
		{
			val &= ~other.val;
			return this;
		}

		public bool Has(ComponentMask other)
		{
			return (other.val & val) == other.val;
		}

		public bool NotHas(ComponentMask other)
		{
			return (other.val & val) == 0;
		}

		public ComponentMask Invert()
		{
			val = ~val;
			return this;
		}

		public ComponentMask Clear()
		{
			val = 0;
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