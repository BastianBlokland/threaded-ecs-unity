using System.Collections.Generic;
using ECS.Storage;

using EntityID = System.UInt16;

namespace ECS.Systems
{
	public abstract class System<Comp1> : System
		where Comp1 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> container1;

		public System(EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.container1 = context.GetContainer<Comp1>();
		}

		public sealed override void Execute(EntityID entity)
		{
			Execute(entity, ref container1.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp);

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>());
		}
	}

	public abstract class System<Comp1, Comp2> : System
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;

		public System(EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
		}

		public sealed override void Execute(EntityID entity)
		{
			Execute(entity, ref container1.Data[entity], ref container2.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp1, ref Comp2 comp2);

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>());
		}
	}

	public abstract class System<Comp1, Comp2, Comp3> : System
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;

		public System(EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
		}

		public sealed override void Execute(EntityID entity)
		{
			Execute(entity, 
				ref container1.Data[entity],
				ref container2.Data[entity],
				ref container3.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp1, ref Comp2 comp2, ref Comp3 comp3);

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>())
				.Add(context.GetMask<Comp3>());
		}
	}

	public abstract class System<Comp1, Comp2, Comp3, Comp4> : System
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
		where Comp4 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;
		private readonly IComponentContainer<Comp4> container4;

		public System(EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
			this.container4 = context.GetContainer<Comp4>();
		}

		public sealed override void Execute(EntityID entity)
		{
			Execute(entity, 
				ref container1.Data[entity],
				ref container2.Data[entity],
				ref container3.Data[entity],
				ref container4.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, 
			ref Comp1 comp1, 
			ref Comp2 comp2, 
			ref Comp3 comp3,
			ref Comp4 comp4);

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>())
				.Add(context.GetMask<Comp3>())
				.Add(context.GetMask<Comp4>());
		}
	}

	public abstract class System<Comp1, Comp2, Comp3, Comp4, Comp5> : System
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
		where Comp4 : struct, IComponent
		where Comp5 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;
		private readonly IComponentContainer<Comp4> container4;
		private readonly IComponentContainer<Comp5> container5;

		public System(EntityContext context, int batchSize) : base(context, batchSize)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
			this.container4 = context.GetContainer<Comp4>();
			this.container5 = context.GetContainer<Comp5>();
		}

		public sealed override void Execute(EntityID entity)
		{
			Execute(entity, 
				ref container1.Data[entity],
				ref container2.Data[entity],
				ref container3.Data[entity],
				ref container4.Data[entity],
				ref container5.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, 
			ref Comp1 comp1, 
			ref Comp2 comp2, 
			ref Comp3 comp3,
			ref Comp4 comp4,
			ref Comp5 comp5);

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>())
				.Add(context.GetMask<Comp3>())
				.Add(context.GetMask<Comp4>())
				.Add(context.GetMask<Comp5>());
		}
	}

    public abstract class System
    {
		public readonly int BatchSize;

		private readonly EntityContext context;
		private readonly IList<EntityID> entities;

		private readonly ComponentMask requiredComponents;
		private readonly ComponentMask illegalComponents;

		public System(EntityContext context, int batchSize)
		{
			this.context = context;
			this.entities = new List<EntityID>();
			
			BatchSize = batchSize;
			
			requiredComponents = GetRequiredComponents(context);
			illegalComponents = GetIllegalComponents(context);
		}

		/// <summary>
		/// NOTE: The reference returned here is re-used between calls to avoid having to re-allocate 
		/// for every frame, so beware to threat this a very short lived data as another call to 'GetEntities'
		/// will change the data
		/// </summary>
		public IList<EntityID> GetEntities()
		{
			context.GetEntities(requiredComponents, illegalComponents, entities);
			return entities;
		}

		public abstract void Execute(EntityID entity);

		protected virtual ComponentMask GetRequiredComponents(EntityContext context)
		{
			return ComponentMask.Empty;
		}

		protected virtual ComponentMask GetIllegalComponents(EntityContext context)
		{
			return ComponentMask.Empty;
		}
    }
}