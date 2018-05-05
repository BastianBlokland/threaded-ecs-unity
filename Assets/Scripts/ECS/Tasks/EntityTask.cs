using System.Collections.Generic;
using ECS.Storage;
using ECS.Tasks.Runner;

using EntityID = System.UInt16;

namespace ECS.Tasks
{
	public abstract class EntityTask<Comp1> : EntityTask
		where Comp1 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
		}

		protected sealed override void Execute(EntityID entity)
		{
			Execute(entity, ref container1.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp);

		protected override TagMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context) 
				+ context.GetMask<Comp1>();
		}
	}

	public abstract class EntityTask<Comp1, Comp2> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
		}

		protected sealed override void Execute(EntityID entity)
		{
			Execute(entity, ref container1.Data[entity], ref container2.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp1, ref Comp2 comp2);

		protected override TagMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context) 
				+ context.GetMask<Comp1>() 
				+ context.GetMask<Comp2>();
		}
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
		where Comp3 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
		}

		protected sealed override void Execute(EntityID entity)
		{
			Execute(entity, 
				ref container1.Data[entity],
				ref container2.Data[entity],
				ref container3.Data[entity]);
		}

		protected abstract void Execute(EntityID entity, ref Comp1 comp1, ref Comp2 comp2, ref Comp3 comp3);

		protected override TagMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context) + context.GetMask<Comp1>() + context.GetMask<Comp2>() + context.GetMask<Comp3>();
		}
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3, Comp4> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
		where Comp3 : struct, IDataComponent
		where Comp4 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;
		private readonly IComponentContainer<Comp4> container4;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
			this.container4 = context.GetContainer<Comp4>();
		}

		protected sealed override void Execute(EntityID entity)
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

		protected override TagMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				+ context.GetMask<Comp1>()
				+ context.GetMask<Comp2>()
				+ context.GetMask<Comp3>()
				+ context.GetMask<Comp4>();
		}
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3, Comp4, Comp5> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
		where Comp3 : struct, IDataComponent
		where Comp4 : struct, IDataComponent
		where Comp5 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;
		private readonly IComponentContainer<Comp4> container4;
		private readonly IComponentContainer<Comp5> container5;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
			this.container3 = context.GetContainer<Comp3>();
			this.container4 = context.GetContainer<Comp4>();
			this.container5 = context.GetContainer<Comp5>();
		}

		protected sealed override void Execute(EntityID entity)
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

		protected override TagMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				+ context.GetMask<Comp1>()
				+ context.GetMask<Comp2>()
				+ context.GetMask<Comp3>()
				+ context.GetMask<Comp4>()
				+ context.GetMask<Comp5>();
		}
	}

    public abstract class EntityTask : SubtaskExecutor
    {
		private readonly EntityContext context;
		private readonly EntitySet entities;

		private readonly TagMask requiredComponents;
		private readonly TagMask illegalComponents;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(runner, batchSize, profiler)
		{
			this.context = context;
			this.entities = new EntitySet();
			
			requiredComponents = GetRequiredComponents(context);
			illegalComponents = GetIllegalComponents(context);
		}

		protected virtual TagMask GetRequiredComponents(EntityContext context)
		{
			return TagMask.Empty;
		}

		protected virtual TagMask GetIllegalComponents(EntityContext context)
		{
			return TagMask.Empty;
		}

		protected sealed override int PrepareSubtasks()
		{
			context.GetEntities(requiredComponents, illegalComponents, entities);
			return entities.Count;
		}

		protected sealed override void ExecuteSubtask(int index)
		{
			EntityID entity = entities.Data[index];
			Execute(entity);
		}

		protected abstract void Execute(EntityID entity);
	}
}