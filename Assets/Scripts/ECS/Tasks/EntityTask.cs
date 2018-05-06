using ECS.Storage;
using ECS.Tasks.Runner;

using EntityID = System.UInt16;

namespace ECS.Tasks
{
	public abstract class EntityTask<Comp1> : EntityTask
		where Comp1 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> cont1;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.cont1 = context.GetContainer<Comp1>();
		}

		protected sealed override void Execute(int execID, EntityID entity) 
			=> Execute(execID, entity, ref cont1.Data[entity]);

		protected abstract void Execute(int execID, EntityID entity, ref Comp1 comp);

		protected override TagMask GetRequiredTags(EntityContext context) 
			=> base.GetRequiredTags(context) + context.GetMask<Comp1>();
	}

	public abstract class EntityTask<Comp1, Comp2> : EntityTask
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> cont1;
		private readonly IComponentContainer<Comp2> cont2;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.cont1 = context.GetContainer<Comp1>();
			this.cont2 = context.GetContainer<Comp2>();
		}

		protected sealed override void Execute(int execID, EntityID entity) 
			=> Execute(execID, entity, ref cont1.Data[entity], ref cont2.Data[entity]);

		protected abstract void Execute(int execID, EntityID entity, ref Comp1 comp1, ref Comp2 comp2);

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context)
			+ context.GetMask<Comp1>()
			+ context.GetMask<Comp2>();
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3> : EntityTask
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> cont1;
		private readonly IComponentContainer<Comp2> cont2;
		private readonly IComponentContainer<Comp3> cont3;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.cont1 = context.GetContainer<Comp1>();
			this.cont2 = context.GetContainer<Comp2>();
			this.cont3 = context.GetContainer<Comp3>();
		}

		protected sealed override void Execute(int execID, EntityID entity)
			=> Execute(execID, entity, 
			ref cont1.Data[entity],
			ref cont2.Data[entity],
			ref cont3.Data[entity]);

		protected abstract void Execute(int execID, EntityID entity, ref Comp1 comp1, ref Comp2 comp2, ref Comp3 comp3);

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context) 
			+ context.GetMask<Comp1>() 
			+ context.GetMask<Comp2>() 
			+ context.GetMask<Comp3>();
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3, Comp4> : EntityTask
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
		where Comp4 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> cont1;
		private readonly IComponentContainer<Comp2> cont2;
		private readonly IComponentContainer<Comp3> cont3;
		private readonly IComponentContainer<Comp4> cont4;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.cont1 = context.GetContainer<Comp1>();
			this.cont2 = context.GetContainer<Comp2>();
			this.cont3 = context.GetContainer<Comp3>();
			this.cont4 = context.GetContainer<Comp4>();
		}

		protected sealed override void Execute(int execID, EntityID entity)
			=> Execute(execID, entity, 
			ref cont1.Data[entity],
			ref cont2.Data[entity],
			ref cont3.Data[entity],
			ref cont4.Data[entity]);

		protected abstract void Execute(int execID, EntityID entity, 
			ref Comp1 comp1, 
			ref Comp2 comp2, 
			ref Comp3 comp3,
			ref Comp4 comp4);

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context)
			+ context.GetMask<Comp1>()
			+ context.GetMask<Comp2>()
			+ context.GetMask<Comp3>()
			+ context.GetMask<Comp4>();
	}

	public abstract class EntityTask<Comp1, Comp2, Comp3, Comp4, Comp5> : EntityTask
		where Comp1 : struct, IComponent
		where Comp2 : struct, IComponent
		where Comp3 : struct, IComponent
		where Comp4 : struct, IComponent
		where Comp5 : struct, IComponent
	{
		private readonly IComponentContainer<Comp1> cont1;
		private readonly IComponentContainer<Comp2> cont2;
		private readonly IComponentContainer<Comp3> cont3;
		private readonly IComponentContainer<Comp4> cont4;
		private readonly IComponentContainer<Comp5> cont5;

		public EntityTask(EntityContext context, SubtaskRunner runner, int batchSize, Profiler.Timeline profiler = null)
			: base(context, runner, batchSize, profiler)
		{
			this.cont1 = context.GetContainer<Comp1>();
			this.cont2 = context.GetContainer<Comp2>();
			this.cont3 = context.GetContainer<Comp3>();
			this.cont4 = context.GetContainer<Comp4>();
			this.cont5 = context.GetContainer<Comp5>();
		}

		protected sealed override void Execute(int execID, EntityID entity)
			=> Execute(execID, entity, 
			ref cont1.Data[entity],
			ref cont2.Data[entity],
			ref cont3.Data[entity],
			ref cont4.Data[entity],
			ref cont5.Data[entity]);

		protected abstract void Execute(int execID, EntityID entity, 
			ref Comp1 comp1, 
			ref Comp2 comp2, 
			ref Comp3 comp3,
			ref Comp4 comp4,
			ref Comp5 comp5);

		protected override TagMask GetRequiredTags(EntityContext context)
			=> base.GetRequiredTags(context)
			+ context.GetMask<Comp1>()
			+ context.GetMask<Comp2>()
			+ context.GetMask<Comp3>()
			+ context.GetMask<Comp4>()
			+ context.GetMask<Comp5>();
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
			
			requiredComponents = GetRequiredTags(context);
			illegalComponents = GetIllegalTags(context);
		}

		protected virtual TagMask GetRequiredTags(EntityContext context) => TagMask.Empty;

		protected virtual TagMask GetIllegalTags(EntityContext context) => TagMask.Empty;

		protected sealed override int PrepareSubtasks()
		{
			context.GetEntities(requiredComponents, illegalComponents, entities);
			return entities.Count;
		}

		protected sealed override void ExecuteSubtask(int execID, int index) => 
			Execute(execID, entities.Data[index]);

		protected abstract void Execute(int execID, EntityID entity);
	}
}