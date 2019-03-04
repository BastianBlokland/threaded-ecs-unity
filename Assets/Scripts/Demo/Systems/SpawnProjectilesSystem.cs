using ECS.Storage;
using ECS.Tasks;
using UnityEngine;
using Utils;
using Utils.Random;

using EntityID = System.UInt16;
using static Utils.MathUtils;

namespace Demo
{
    public sealed class SpawnProjectilesSystem : EntityTask<ProjectileSpawnerComponent, TransformComponent>
    {
        private readonly ColliderManager colliderManager;
        private readonly DeltaTimeHandle deltaTime;
        private readonly EntityContext context;
        private readonly TagMask disabledMask;

        public SpawnProjectilesSystem(ColliderManager colliderManager, DeltaTimeHandle deltaTime, EntityContext context)
            : base(context, batchSize: 100)
        {
            this.colliderManager = colliderManager;
            this.deltaTime = deltaTime;
            this.context = context;
            this.disabledMask = context.GetMask<DisabledTag>();
        }

        protected override void Execute(int execID, EntityID entity, ref ProjectileSpawnerComponent spawner, ref TransformComponent trans)
        {
            const int SHOTS_PER_BURST = 5;
            const float COOLDOWN_AFTER_BURST = 1.5f;
            const float COOLDOWN_BETWEEN_SHOTS = .05f;

            spawner.Cooldown -= deltaTime.Value;
            if(spawner.Cooldown > 0f) //Still cooling down so early out
                return;

            if(spawner.Target != null && context.HasEntity(spawner.Target.Value) && !context.HasTags(spawner.Target.Value, disabledMask))
                FireProjectile(ref spawner, ref trans);
            spawner.Cooldown = COOLDOWN_BETWEEN_SHOTS;
            spawner.ShotsRemaining--;

            //If there are no shots-remaining then start cooling down
            if(spawner.ShotsRemaining <= 0)
            {
                spawner.ShotsRemaining = SHOTS_PER_BURST;
                spawner.Cooldown = COOLDOWN_AFTER_BURST;

                EntityID target;
                if(colliderManager.Intersect(AABox.FromCenterAndExtents(trans.Matrix.Position, new Vector3(75f, 125f, 75f)), out target))
                    spawner.Target = target;
            }
        }

        private void FireProjectile(ref ProjectileSpawnerComponent spawner, ref TransformComponent turretTrans)
        {
            const float INVERSE_PROJECTILE_SPEED = 1f / 100f;

            Vector3 turretPos = turretTrans.Matrix.Position;
            Vector3 targetPos = context.GetComponent<TransformComponent>(spawner.Target.Value).Matrix.Position;
            Vector3 targetVelo = context.GetComponent<VelocityComponent>(spawner.Target.Value).Velocity;

            float distanceEst = ManhattanDistance(targetPos - turretPos);
            float flightTime = distanceEst * INVERSE_PROJECTILE_SPEED;
            if(flightTime <= 0f)
                return;

            //Aim ahead of the target to compensation for its movement
            Vector3 aimTarget = targetPos + targetVelo * flightTime;

            //Gather info about the trajectory from the turret to the aim-target
            Vector3 toAimTarget = aimTarget - turretPos;
            float toAimTargetSqrDist = toAimTarget.sqrMagnitude;
            float toAimTargetDist = Mathf.Sqrt(toAimTargetSqrDist); //TODO: See if we can get rid of the SquareRoot usage here
            Vector3 toAimTargetDir = toAimTarget * FastInvSqrRoot(toAimTargetSqrDist); //Create normalized vector aiming to toward the aim target

            //Speed that the projectile will be traveling at. (NOTE: Its not the same as the configured projectile speed we used
            //to estimate the flighttime, thats because the estimate did not compensate for the target velocity yet)
            float trajectorySpeed = toAimTargetDist / flightTime;

            //Calculate how much gravity we will encounter in the journey
            float gravity = ApplyGravitySystem.GRAVITY * flightTime;

            //Calculate the 'final' projectileVelocity and include the gravity compensation
            Vector3 projectileVelocity = toAimTargetDir * trajectorySpeed + Vector3.down * (gravity * .5f);

            //This is used to 'point' the turret, if we want to be cheap we could use the 'toAimTargetDir' but thats not
            //really accurate because it doesn't include the gravity compensation yet
            Vector3 velocityDir = FastNormalize(projectileVelocity);

            //Spawn the projectile entity
            var entity = context.CreateEntity();
            context.SetComponent(entity, new TransformComponent(Float3x4.FromPosition(turretPos)));
            context.SetComponent(entity, new VelocityComponent(velocity: projectileVelocity));
            context.SetComponent(entity, new GraphicComponent(graphicID: 2));
            context.SetComponent(entity, new AgeComponent());
            context.SetComponent(entity, new LifetimeComponent(totalLifetime: 3f));
            context.SetTag<ProjectileTag>(entity);
            context.SetTag<ApplyGravityTag>(entity);

            //Update the turret to face the target
            turretTrans.Matrix = Float3x4.FromPositionAndForward(turretTrans.Matrix.Position, velocityDir);
        }
    }
}
