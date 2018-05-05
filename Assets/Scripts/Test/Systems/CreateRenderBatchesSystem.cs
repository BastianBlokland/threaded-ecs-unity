﻿using Utils;
using ECS.Storage;
using ECS.Tasks;
using ECS.Tasks.Runner;
using UnityEngine;
using Test.Components;

using EntityID = System.UInt16;

namespace Test.Systems
{
    public class CreateRenderBatchesSystem : EntityTask<GraphicsComponent, TransformComponent>
    {
		private readonly RenderSet renderSet;

		public CreateRenderBatchesSystem(RenderSet renderSet, 
			EntityContext context, SubtaskRunner runner, Profiler.Timeline profiler) : base(context, runner, 100, profiler)
		{
			this.renderSet = renderSet;
		}

        protected override void Execute(EntityID entity, ref GraphicsComponent graphics, ref TransformComponent trans)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(trans.Position, trans.Rotation, Vector3.one);
			renderSet.Add(graphics.GraphicsID, matrix);
		}
    }
}