using System.Collections.Generic;
using ECS.Storage;

using EntityID = System.UInt16;

namespace ECS.Tasks
{
	public abstract class EntityTask<Comp1> : EntityTask
		where Comp1 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
			: base(context, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
		}

		protected sealed override void Execute(EntityID entity)
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

	public abstract class EntityTask<Comp1, Comp2> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
			: base(context, batchSize, profiler)
		{
			this.container1 = context.GetContainer<Comp1>();
			this.container2 = context.GetContainer<Comp2>();
		}

		protected sealed override void Execute(EntityID entity)
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

	public abstract class EntityTask<Comp1, Comp2, Comp3> : EntityTask
		where Comp1 : struct, IDataComponent
		where Comp2 : struct, IDataComponent
		where Comp3 : struct, IDataComponent
	{
		private readonly IComponentContainer<Comp1> container1;
		private readonly IComponentContainer<Comp2> container2;
		private readonly IComponentContainer<Comp3> container3;

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
			: base(context, batchSize, profiler)
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

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>())
				.Add(context.GetMask<Comp3>());
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

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
			: base(context, batchSize, profiler)
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

		protected override ComponentMask GetRequiredComponents(EntityContext context)
		{
			return base.GetRequiredComponents(context)
				.Add(context.GetMask<Comp1>())
				.Add(context.GetMask<Comp2>())
				.Add(context.GetMask<Comp3>())
				.Add(context.GetMask<Comp4>());
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

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
			: base(context, batchSize, profiler)
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

    public abstract class EntityTask : ITask, SingleTaskExecutor.IExecutableTask
    {
		private readonly EntityContext context;
		private readonly int batchSize;
		private readonly EntitySet entities;

		private readonly ComponentMask requiredComponents;
		private readonly ComponentMask illegalComponents;

		private readonly Profiler.TimelineTrack profilerTrack;

		public EntityTask(EntityContext context, int batchSize, Profiler.Timeline profiler = null)
		{
			this.context = context;
			this.batchSize = batchSize;
			this.entities = new EntitySet();
			
			requiredComponents = GetRequiredComponents(context);
			illegalComponents = GetIllegalComponents(context);

			if(profiler != null)
				profilerTrack = profiler.CreateTrack<Profiler.TimelineTrack>(label: GetType().Name);
		}

		public ITaskExecutor CreateExecutor(Runner.SubtaskRunner runner)
		{
			return new SingleTaskExecutor(this, runner, batchSize, profilerTrack);
		}

		protected virtual ComponentMask GetRequiredComponents(EntityContext context)
		{
			return ComponentMask.Empty;
		}

		protected virtual ComponentMask GetIllegalComponents(EntityContext context)
		{
			return ComponentMask.Empty;
		}

		int SingleTaskExecutor.IExecutableTask.PrepareSubtasks()
		{
			context.GetEntities(requiredComponents, illegalComponents, entities);
			return entities.Count;
		}

		void SingleTaskExecutor.IExecutableTask.ExecuteSubtask(int index)
		{
			EntityID entity = entities.Data[index];
			Execute(entity);
		}

		protected abstract void Execute(EntityID entity);
	}
}