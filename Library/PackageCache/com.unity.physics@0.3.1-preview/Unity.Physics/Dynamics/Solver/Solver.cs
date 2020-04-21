﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;
using static Unity.Physics.Math;

namespace Unity.Physics
{
    public static class Solver
    {
        internal struct StepInput
        {
            public bool IsLastIteration;
            public float InvNumSolverIterations;
            public float Timestep;
            public float InvTimestep;
        }

        // Apply gravity to all dynamic bodies and copy input velocities
        public static void ApplyGravityAndCopyInputVelocities(NativeSlice<MotionData> motionDatas, NativeSlice<MotionVelocity> motionVelocities,
            NativeSlice<Velocity> inputVelocities, float3 gravityAcceleration)
        {
            for (int i = 0; i < motionDatas.Length; i++)
            {
                ParallelApplyGravityAndCopyInputVelocitiesJob.ExecuteImpl(i, gravityAcceleration, motionDatas, motionVelocities, inputVelocities);
            }
        }

        // Schedule the job to apply gravity to all dynamic bodies and copy input velocities
        internal static JobHandle ScheduleApplyGravityAndCopyInputVelocitiesJob(ref DynamicsWorld world, NativeSlice<Velocity> inputVelocities,
            float3 gravityAcceleration, JobHandle inputDeps, int threadCountHint = 0)
        {
            if (threadCountHint <= 0)
            {
                var job = new ApplyGravityAndCopyInputVelocitiesJob
                {
                    MotionDatas = world.MotionDatas,
                    MotionVelocities = world.MotionVelocities,
                    InputVelocities = inputVelocities,
                    GravityAcceleration = gravityAcceleration
                };
                return job.Schedule(inputDeps);
            }
            else
            {
                var job = new ParallelApplyGravityAndCopyInputVelocitiesJob
                {
                    MotionDatas = world.MotionDatas,
                    MotionVelocities = world.MotionVelocities,
                    InputVelocities = inputVelocities,
                    GravityAcceleration = gravityAcceleration
                };
                return job.Schedule(world.NumMotions, 64, inputDeps);
            }
        }

        // Build Jacobians from the contacts and joints stored in the simulation context
        public static void BuildJacobians(ref PhysicsWorld world,
            float timeStep, float3 gravity, int numSolverIterations,
            NativeArray<DispatchPairSequencer.DispatchPair> dispatchPairs,
            ref NativeStream.Reader contactsReader, ref NativeStream.Writer jacobiansWriter)
        {
            contactsReader.BeginForEachIndex(0);
            jacobiansWriter.BeginForEachIndex(0);
            float invTimeStep = timeStep > 0.0f ? 1.0f / timeStep : 0.0f;
            float gravityAcceleration = math.length(gravity);
            BuildJacobians(ref world, timeStep, invTimeStep, gravityAcceleration, numSolverIterations,
                new NativeSlice<DispatchPairSequencer.DispatchPair>(dispatchPairs, 0, dispatchPairs.Length), ref contactsReader, ref jacobiansWriter);
        }

        // Schedule jobs to build Jacobians from the contacts stored in the simulation context
        internal static SimulationJobHandles ScheduleBuildJacobiansJobs(ref PhysicsWorld world, float timeStep, float3 gravity,
            int numSolverIterations, JobHandle inputDeps, ref NativeList<DispatchPairSequencer.DispatchPair> dispatchPairs,
            ref DispatchPairSequencer.SolverSchedulerInfo solverSchedulerInfo,
            ref NativeStream contacts, ref NativeStream jacobians, int threadCountHint = 0)
        {
            SimulationJobHandles returnHandles = default;

            if (threadCountHint <= 0)
            {
                returnHandles.FinalExecutionHandle = new BuildJacobiansJob
                {
                    ContactsReader = contacts.AsReader(),
                    JacobiansWriter = jacobians.AsWriter(),
                    TimeStep = timeStep,
                    Gravity = gravity,
                    NumSolverIterations = numSolverIterations,
                    World = world,
                    DispatchPairs = dispatchPairs.AsDeferredJobArray()
                }.Schedule(inputDeps);
            }
            else
            {
                var buildJob = new ParallelBuildJacobiansJob
                {
                    ContactsReader = contacts.AsReader(),
                    JacobiansWriter = jacobians.AsWriter(),
                    TimeStep = timeStep,
                    InvTimeStep = timeStep > 0.0f ? 1.0f / timeStep : 0.0f,
                    GravityAcceleration = math.length(gravity),
                    NumSolverIterations = numSolverIterations,
                    World = world,
                    DispatchPairs = dispatchPairs.AsDeferredJobArray(),
                    SolverSchedulerInfo = solverSchedulerInfo
                };

                JobHandle handle = buildJob.ScheduleUnsafeIndex0(solverSchedulerInfo.NumWorkItems, 1, inputDeps);

                returnHandles.FinalDisposeHandle = JobHandle.CombineDependencies(
                    dispatchPairs.Dispose(handle),
                    contacts.Dispose(handle));
                returnHandles.FinalExecutionHandle = handle;
            }

            return returnHandles;
        }

        // Solve the Jacobians stored in the simulation context
        public static void SolveJacobians(ref NativeStream.Reader jacobiansReader, NativeSlice<MotionVelocity> motionVelocities, float timeStep, int numIterations,
            ref NativeStream.Writer collisionEventsWriter, ref NativeStream.Writer triggerEventsWriter)
        {
            float invNumIterations = math.rcp(numIterations);
            float invTimeStep = timeStep > 0.0f ? 1.0f / timeStep : 0.0f;
            for (int solverIterationId = 0; solverIterationId < numIterations; solverIterationId++)
            {
                var stepInput = new StepInput
                {
                    InvNumSolverIterations = invNumIterations,
                    IsLastIteration = solverIterationId == numIterations - 1,
                    Timestep = timeStep,
                    InvTimestep = invTimeStep
                };

                Solve(motionVelocities, ref jacobiansReader, ref collisionEventsWriter, ref triggerEventsWriter, 0, stepInput);
            }
        }

        // Schedule jobs to solve the Jacobians stored in the simulation context
        internal static unsafe SimulationJobHandles ScheduleSolveJacobiansJobs(ref DynamicsWorld dynamicsWorld, float timestep, int numIterations,
            ref NativeStream jacobians, ref NativeStream collisionEvents, ref NativeStream triggerEvents,
            ref DispatchPairSequencer.SolverSchedulerInfo solverSchedulerInfo, JobHandle inputDeps, int threadCountHint = 0)
        {
            SimulationJobHandles returnHandles = default;

            if (threadCountHint <= 0)
            {
                collisionEvents = new NativeStream(1, Allocator.Persistent);
                triggerEvents = new NativeStream(1, Allocator.Persistent);
                returnHandles.FinalExecutionHandle = new SolverJob
                {
                    CollisionEventsWriter = collisionEvents.AsWriter(),
                    JacobiansReader = jacobians.AsReader(),
                    NumIterations = numIterations,
                    Timestep = timestep,
                    TriggerEventsWriter = triggerEvents.AsWriter(),
                    MotionVelocities = dynamicsWorld.MotionVelocities
                }.Schedule(inputDeps);

                return returnHandles;
            }

            JobHandle handle;

            int numPhases = solverSchedulerInfo.NumPhases;

            // Use persistent allocator to allow these to live until the start of next step
            {
                NativeArray<int> workItemList = solverSchedulerInfo.NumWorkItems;

                //TODO: Change this to Allocator.TempJob when https://github.com/Unity-Technologies/Unity.Physics/issues/7 is resolved
                JobHandle collisionEventStreamHandle = NativeStream.ScheduleConstruct(out collisionEvents, workItemList, inputDeps, Allocator.Persistent);
                JobHandle triggerEventStreamHandle = NativeStream.ScheduleConstruct(out triggerEvents, workItemList, inputDeps, Allocator.Persistent);

                handle = JobHandle.CombineDependencies(collisionEventStreamHandle, triggerEventStreamHandle);

                float invNumIterations = math.rcp(numIterations);

                var phaseInfoPtrs = (DispatchPairSequencer.SolverSchedulerInfo.SolvePhaseInfo*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(solverSchedulerInfo.PhaseInfo);

                for (int solverIterationId = 0; solverIterationId < numIterations; solverIterationId++)
                {
                    bool lastIteration = solverIterationId == numIterations - 1;
                    for (int phaseId = 0; phaseId < numPhases; phaseId++)
                    {
                        var job = new ParallelSolverJob
                        {
                            JacobiansReader = jacobians.AsReader(),
                            PhaseIndex = phaseId,
                            Phases = solverSchedulerInfo.PhaseInfo,
                            MotionVelocities = dynamicsWorld.MotionVelocities,
                            StepInput = new StepInput
                            {
                                InvNumSolverIterations = invNumIterations,
                                IsLastIteration = lastIteration,
                                Timestep = timestep,
                                InvTimestep = timestep > 0.0f ? 1.0f / timestep : 0.0f
                            }
                        };

                        // Only initialize event writers for last solver iteration jobs
                        if (lastIteration)
                        {
                            job.CollisionEventsWriter = collisionEvents.AsWriter();
                            job.TriggerEventsWriter = triggerEvents.AsWriter();
                        }

                        // NOTE: The last phase must be executed on a single job since it  
                        // int.MaxValue can't be used as batchSize since 19.1 overflows in that case... 
                        bool isLastPhase = phaseId == numPhases - 1;
                        int batchSize = isLastPhase ? (int.MaxValue / 2) : 1;

                        int* numWorkItems = &(phaseInfoPtrs[phaseId].NumWorkItems);
                        handle = job.Schedule(numWorkItems, batchSize, handle);
                    }
                }
            }

            // Dispose processed data
            returnHandles.FinalDisposeHandle = JobHandle.CombineDependencies(
                jacobians.Dispose(handle),
                solverSchedulerInfo.ScheduleDisposeJob(handle));
            returnHandles.FinalExecutionHandle = handle;

            return returnHandles;
        }

        #region Jobs

        [BurstCompile]
        private struct ParallelApplyGravityAndCopyInputVelocitiesJob : IJobParallelFor
        {
            public NativeSlice<MotionData> MotionDatas;
            public NativeSlice<MotionVelocity> MotionVelocities;
            public NativeSlice<Velocity> InputVelocities;
            public float3 GravityAcceleration;

            public void Execute(int i)
            {
                ExecuteImpl(i, GravityAcceleration, MotionDatas, MotionVelocities, InputVelocities);
            }

            internal static void ExecuteImpl(int i, float3 gravityAcceleration,
                NativeSlice<MotionData> motionDatas, NativeSlice<MotionVelocity> motionVelocities, NativeSlice<Velocity> inputVelocities)
            {
                MotionData motionData = motionDatas[i];
                MotionVelocity motionVelocity = motionVelocities[i];

                // Apply gravity
                motionVelocity.LinearVelocity += gravityAcceleration * motionData.GravityFactor;

                // Write back
                motionVelocities[i] = motionVelocity;

                // Make a copy
                inputVelocities[i] = new Velocity
                {
                    Linear = motionVelocity.LinearVelocity,
                    Angular = motionVelocity.AngularVelocity
                };
            }
        }

        [BurstCompile]
        private struct ApplyGravityAndCopyInputVelocitiesJob : IJob
        {
            public NativeSlice<MotionData> MotionDatas;
            public NativeSlice<MotionVelocity> MotionVelocities;
            public NativeSlice<Velocity> InputVelocities;
            public float3 GravityAcceleration;

            public void Execute()
            {
                ApplyGravityAndCopyInputVelocities(MotionDatas, MotionVelocities, InputVelocities, GravityAcceleration);
            }
        }

        [BurstCompile]
        private struct ParallelBuildJacobiansJob : IJobParallelForDefer
        {
            [ReadOnly] public PhysicsWorld World;

            public NativeStream.Reader ContactsReader;
            public NativeStream.Writer JacobiansWriter;
            public float TimeStep;
            [ReadOnly] public NativeArray<DispatchPairSequencer.DispatchPair> DispatchPairs;
            [ReadOnly] public int NumSolverIterations;
            public float InvTimeStep;
            public float GravityAcceleration;
            [ReadOnly] public DispatchPairSequencer.SolverSchedulerInfo SolverSchedulerInfo;

            public void Execute(int workItemIndex)
            {
                int firstDispatchPairIndex = SolverSchedulerInfo.GetWorkItemReadOffset(workItemIndex, out int dispatchPairCount);

                ContactsReader.BeginForEachIndex(workItemIndex);
                JacobiansWriter.BeginForEachIndex(workItemIndex);
                BuildJacobians(ref World, TimeStep, InvTimeStep, GravityAcceleration, NumSolverIterations,
                    new NativeSlice<DispatchPairSequencer.DispatchPair>(DispatchPairs, firstDispatchPairIndex, dispatchPairCount),
                    ref ContactsReader, ref JacobiansWriter);
            }
        }

        [BurstCompile]
        private struct BuildJacobiansJob : IJob
        {
            [ReadOnly] public PhysicsWorld World;

            public NativeStream.Reader ContactsReader;
            public NativeStream.Writer JacobiansWriter;
            public float TimeStep;
            [ReadOnly] public NativeArray<DispatchPairSequencer.DispatchPair> DispatchPairs;
            [ReadOnly] public int NumSolverIterations;
            public float3 Gravity;

            public void Execute()
            {
                BuildJacobians(ref World, TimeStep, Gravity, NumSolverIterations,
                    DispatchPairs, ref ContactsReader, ref JacobiansWriter);
            }
        }

        [BurstCompile]
        [NoAlias]
        private struct ParallelSolverJob : IJobParallelForDefer
        {
            [NativeDisableParallelForRestriction]
            public NativeSlice<MotionVelocity> MotionVelocities;

            [NoAlias]
            public NativeStream.Reader JacobiansReader;

            //@TODO: Unity should have a Allow null safety restriction
            [NativeDisableContainerSafetyRestriction]
            [NoAlias]
            public NativeStream.Writer CollisionEventsWriter;

            //@TODO: Unity should have a Allow null safety restriction
            [NativeDisableContainerSafetyRestriction]
            [NoAlias]
            public NativeStream.Writer TriggerEventsWriter;

            [ReadOnly]
            public NativeArray<DispatchPairSequencer.SolverSchedulerInfo.SolvePhaseInfo> Phases;
            public int PhaseIndex;
            public StepInput StepInput;

            public void Execute(int workItemIndex)
            {
                int workItemStartIndexOffset = Phases[PhaseIndex].FirstWorkItemIndex;

                CollisionEventsWriter.PatchMinMaxRange(workItemIndex + workItemStartIndexOffset);
                TriggerEventsWriter.PatchMinMaxRange(workItemIndex + workItemStartIndexOffset);

                Solve(MotionVelocities, ref JacobiansReader, ref CollisionEventsWriter, ref TriggerEventsWriter, workItemIndex + workItemStartIndexOffset, StepInput);
            }
        }

        [BurstCompile]
        [NoAlias]
        private struct SolverJob : IJob
        {
            [NativeDisableParallelForRestriction]
            public NativeSlice<MotionVelocity> MotionVelocities;

            [NoAlias]
            public NativeStream.Reader JacobiansReader;

            //@TODO: Unity should have a Allow null safety restriction
            [NativeDisableContainerSafetyRestriction]
            [NoAlias]
            public NativeStream.Writer CollisionEventsWriter;

            //@TODO: Unity should have a Allow null safety restriction
            [NativeDisableContainerSafetyRestriction]
            [NoAlias]
            public NativeStream.Writer TriggerEventsWriter;

            public int NumIterations;
            public float Timestep;

            public void Execute()
            {
                SolveJacobians(ref JacobiansReader, MotionVelocities, Timestep, NumIterations, ref CollisionEventsWriter, ref TriggerEventsWriter);
            }
        }

        #endregion

        #region Implementation

        private static void BuildJacobian(MTransform worldFromA, MTransform worldFromB, float3 normal, float3 armA, float3 armB,
        float3 invInertiaA, float3 invInertiaB, float sumInvMass, out float3 angularA, out float3 angularB, out float invEffectiveMass)
        {
            float3 crossA = math.cross(armA, normal);
            angularA = math.mul(worldFromA.InverseRotation, crossA).xyz;

            float3 crossB = math.cross(normal, armB);
            angularB = math.mul(worldFromB.InverseRotation, crossB).xyz;

            float3 temp = angularA * angularA * invInertiaA + angularB * angularB * invInertiaB;
            invEffectiveMass = temp.x + temp.y + temp.z + sumInvMass;
        }

        private static void BuildContactJacobian(
            int contactPointIndex,
            float3 normal,
            MTransform worldFromA,
            MTransform worldFromB,
            float invTimestep,
            MotionVelocity velocityA,
            MotionVelocity velocityB,
            float sumInvMass,
            float maxDepenetrationVelocity,
            ref JacobianHeader jacobianHeader,
            ref float3 centerA,
            ref float3 centerB,
            ref NativeStream.Reader contactReader)
        {
            ref ContactJacAngAndVelToReachCp jacAngular = ref jacobianHeader.AccessAngularJacobian(contactPointIndex);
            ContactPoint contact = contactReader.Read<ContactPoint>();
            float3 pointOnB = contact.Position;
            float3 pointOnA = contact.Position + normal * contact.Distance;
            float3 armA = pointOnA - worldFromA.Translation;
            float3 armB = pointOnB - worldFromB.Translation;
            BuildJacobian(worldFromA, worldFromB, normal, armA, armB, velocityA.InverseInertia, velocityB.InverseInertia, sumInvMass,
                out jacAngular.Jac.AngularA, out jacAngular.Jac.AngularB, out float invEffectiveMass);
            jacAngular.Jac.EffectiveMass = 1.0f / invEffectiveMass;
            jacAngular.Jac.Impulse = 0.0f;

            float solveDistance = contact.Distance;
            float solveVelocity = solveDistance * invTimestep;

            solveVelocity = math.max(-maxDepenetrationVelocity, solveVelocity);

            jacAngular.VelToReachCp = -solveVelocity;

            // Calculate average position for friction
            centerA += armA;
            centerB += armB;

            // Write the contact point to the jacobian stream if requested
            if (jacobianHeader.HasContactManifold)
            {
                jacobianHeader.AccessContactPoint(contactPointIndex) = contact;
            }
        }

        private static void InitModifierData(ref JacobianHeader jacobianHeader, ColliderKeyPair colliderKeys)
        {
            if (jacobianHeader.HasContactManifold)
            {
                jacobianHeader.AccessColliderKeys() = colliderKeys;
            }
            if (jacobianHeader.HasSurfaceVelocity)
            {
                jacobianHeader.AccessSurfaceVelocity() = new SurfaceVelocity();
            }
            if (jacobianHeader.HasMassFactors)
            {
                jacobianHeader.AccessMassFactors() = MassFactors.Default;
            }
        }

        private static void GetMotions(
            BodyIndexPair pair,
            ref NativeSlice<MotionData> motionDatas,
            ref NativeSlice<MotionVelocity> motionVelocities,
            out MotionVelocity velocityA,
            out MotionVelocity velocityB,
            out MTransform worldFromA,
            out MTransform worldFromB)
        {
            bool bodyAIsStatic = pair.BodyAIndex >= motionVelocities.Length;
            bool bodyBIsStatic = pair.BodyBIndex >= motionVelocities.Length;

            if (bodyAIsStatic)
            {
                if (bodyBIsStatic)
                {
                    Assert.IsTrue(false); // static-static pairs should have been filtered during broadphase overlap test
                    velocityA = MotionVelocity.Zero;
                    velocityB = MotionVelocity.Zero;
                    worldFromA = MTransform.Identity;
                    worldFromB = MTransform.Identity;
                    return;
                }

                velocityA = MotionVelocity.Zero;
                velocityB = motionVelocities[pair.BodyBIndex];

                worldFromA = MTransform.Identity;
                worldFromB = new MTransform(motionDatas[pair.BodyBIndex].WorldFromMotion);
            }
            else if (bodyBIsStatic)
            {
                velocityA = motionVelocities[pair.BodyAIndex];
                velocityB = MotionVelocity.Zero;

                worldFromA = new MTransform(motionDatas[pair.BodyAIndex].WorldFromMotion);
                worldFromB = MTransform.Identity;
            }
            else
            {
                velocityA = motionVelocities[pair.BodyAIndex];
                velocityB = motionVelocities[pair.BodyBIndex];

                worldFromA = new MTransform(motionDatas[pair.BodyAIndex].WorldFromMotion);
                worldFromB = new MTransform(motionDatas[pair.BodyBIndex].WorldFromMotion);
            }
        }

        // Gets a body's motion, even if the body is static
        // TODO - share code with GetMotions()?
        private static void GetMotion([NoAlias] ref PhysicsWorld world, int bodyIndex,
            [NoAlias] out MotionVelocity velocity, [NoAlias] out MotionData motion)
        {
            if (bodyIndex >= world.MotionVelocities.Length)
            {
                // Body is static
                velocity = MotionVelocity.Zero;
                motion = new MotionData
                {
                    WorldFromMotion = world.Bodies[bodyIndex].WorldFromBody,
                    BodyFromMotion = RigidTransform.identity
                    // remaining fields all zero
                };
            }
            else
            {
                // Body is dynamic
                velocity = world.MotionVelocities[bodyIndex];
                motion = world.MotionDatas[bodyIndex];
            }
        }

        private static unsafe void BuildJacobians(
            ref PhysicsWorld world,
            float timestep,
            float invTimestep,
            float gravityAcceleration,
            int numSolverIterations,
            NativeSlice<DispatchPairSequencer.DispatchPair> dispatchPairs,
            ref NativeStream.Reader contactReader,
            ref NativeStream.Writer jacobianWriter)
        {
            // Contact resting velocity for restitution
            float negContactRestingVelocity = -gravityAcceleration * timestep;

            for (int i = 0; i < dispatchPairs.Length; i++)
            {
                var pair = dispatchPairs[i];
                if (!pair.IsValid)
                {
                    continue;
                }

                var motionDatas = world.MotionDatas;
                var motionVelocities = world.MotionVelocities;

                if (pair.IsContact)
                {
                    while (contactReader.RemainingItemCount > 0)
                    {
                        // Check if this is the matching contact
                        {
                            var header = contactReader.Peek<ContactHeader>();
                            if (pair.BodyAIndex != header.BodyPair.BodyAIndex ||
                                pair.BodyBIndex != header.BodyPair.BodyBIndex)
                            {
                                break;
                            }
                        }

                        ref ContactHeader contactHeader = ref contactReader.Read<ContactHeader>();
                        GetMotions(contactHeader.BodyPair, ref motionDatas, ref motionVelocities, out MotionVelocity velocityA, out MotionVelocity velocityB, out MTransform worldFromA, out MTransform worldFromB);

                        float sumInvMass = velocityA.InverseMass + velocityB.InverseMass;
                        bool bodiesHaveInfiniteMass = velocityA.HasInfiniteInertiaAndMass && velocityB.HasInfiniteInertiaAndMass;

                        // Skip contact between infinite mass bodies which don't want to raise events. These cannot have any effect during solving.
                        // These should not normally appear, because the collision detector doesn't generate such contacts.
                        if (bodiesHaveInfiniteMass)
                        {
                            if ((contactHeader.JacobianFlags & (JacobianFlags.IsTrigger | JacobianFlags.EnableCollisionEvents)) == 0)
                            {
                                for (int j = 0; j < contactHeader.NumContacts; j++)
                                {
                                    contactReader.Read<ContactPoint>();
                                }
                                continue;
                            }
                        }

                        JacobianType jacType = ((int)(contactHeader.JacobianFlags) & (int)(JacobianFlags.IsTrigger)) != 0 ?
                            JacobianType.Trigger : JacobianType.Contact;

                        // Write size before every jacobian and allocate all necessary data for this jacobian
                        int jacobianSize = JacobianHeader.CalculateSize(jacType, contactHeader.JacobianFlags, contactHeader.NumContacts);
                        jacobianWriter.Write(jacobianSize);
                        byte* jacobianPtr = jacobianWriter.Allocate(jacobianSize);

#if DEVELOPMENT_BUILD
                if (((long)jacobianPtr & 0x3) != 0)
                    throw new InvalidOperationException("Jacobians must be 4 byte aligned");
#endif
                        ref JacobianHeader jacobianHeader = ref UnsafeUtilityEx.AsRef<JacobianHeader>(jacobianPtr);
                        jacobianHeader.BodyPair = contactHeader.BodyPair;
                        jacobianHeader.Type = jacType;
                        jacobianHeader.Flags = contactHeader.JacobianFlags;

                        var baseJac = new BaseContactJacobian
                        {
                            NumContacts = contactHeader.NumContacts,
                            Normal = contactHeader.Normal
                        };

                        // Body A must be dynamic
                        Assert.IsTrue(contactHeader.BodyPair.BodyAIndex < motionVelocities.Length);
                        bool isDynamicStaticPair = contactHeader.BodyPair.BodyBIndex >= motionVelocities.Length;

                        // If contact distance is negative, use an artificially reduced penetration depth to prevent the dynamic-dynamic contacts from depenetrating too quickly
                        float maxDepenetrationVelocity = isDynamicStaticPair ? float.MaxValue : 3.0f; // meter/seconds time step independent

                        if (jacobianHeader.Type == JacobianType.Contact)
                        {
                            ref ContactJacobian contactJacobian = ref jacobianHeader.AccessBaseJacobian<ContactJacobian>();
                            contactJacobian.BaseJacobian = baseJac;
                            contactJacobian.CoefficientOfFriction = contactHeader.CoefficientOfFriction;

                            // Indicator whether restitution will be applied,
                            // used to scale down friction on bounce.
                            bool applyRestitution = false;

                            // Initialize modifier data (in order from JacobianModifierFlags) before angular jacobians
                            InitModifierData(ref jacobianHeader, contactHeader.ColliderKeys);

                            // Build normal jacobians
                            var centerA = new float3(0.0f);
                            var centerB = new float3(0.0f);
                            for (int j = 0; j < contactHeader.NumContacts; j++)
                            {
                                // Build the jacobian
                                BuildContactJacobian(
                                    j, contactJacobian.BaseJacobian.Normal, worldFromA, worldFromB, invTimestep, velocityA, velocityB, sumInvMass, maxDepenetrationVelocity,
                                    ref jacobianHeader, ref centerA, ref centerB, ref contactReader);

                                // Restitution (optional)
                                if (contactHeader.CoefficientOfRestitution > 0.0f)
                                {
                                    ref ContactJacAngAndVelToReachCp jacAngular = ref jacobianHeader.AccessAngularJacobian(j);
                                    float relativeVelocity = BaseContactJacobian.GetJacVelocity(baseJac.Normal, jacAngular.Jac, velocityA, velocityB);
                                    float dv = jacAngular.VelToReachCp - relativeVelocity;
                                    if (dv > 0.0f && relativeVelocity < negContactRestingVelocity)
                                    {
                                        // Restitution impulse is applied as if contact point is on the contact plane.
                                        // However, it can (and will) be slightly away from contact plane at the moment restitution is applied.
                                        // So we have to apply vertical shot equation to make sure we don't gain energy:
                                        // effectiveRestitutionVelocity^2 = restitutionVelocity^2 - 2.0f * gravityAcceleration * distanceToGround
                                        // From this formula we calculate the effective restitution velocity, which is the velocity 
                                        // that the contact point needs to reach the same height from current position
                                        // as if it was shot with the restitutionVelocity from the contact plane.
                                        // ------------------------------------------------------------
                                        // This is still an approximation for 2 reasons:
                                        // - We are assuming the contact point will hit the contact plane with its current velocity,
                                        // while actually it would have a portion of gravity applied before the actual hit. However,
                                        // that velocity increase is quite small (less than gravity in one step), so it's safe
                                        // to use current velocity instead.
                                        // - gravityAcceleration is the actual value of gravity applied only when contact plane is
                                        // directly opposite to gravity direction. Otherwise, this value will only be smaller.
                                        // However, since this can only result in smaller bounce than the "correct" one, we can
                                        // safely go with the default gravity value in all cases.
                                        float restitutionVelocity = (relativeVelocity - negContactRestingVelocity) * contactHeader.CoefficientOfRestitution;
                                        float distanceToGround = math.max(-jacAngular.VelToReachCp * timestep, 0.0f);
                                        float effectiveRestitutionVelocity =
                                            math.sqrt(math.max(restitutionVelocity * restitutionVelocity - 2.0f * gravityAcceleration * distanceToGround, 0.0f));

                                        jacAngular.VelToReachCp =
                                            math.max(jacAngular.VelToReachCp - effectiveRestitutionVelocity, 0.0f) +
                                            effectiveRestitutionVelocity;

                                        // Remember that restitution should be applied
                                        applyRestitution = true;
                                    }
                                }
                            }

                            // Build friction jacobians
                            // (skip friction between two infinite-mass objects)
                            if (!bodiesHaveInfiniteMass)
                            {
                                // Clear accumulated impulse
                                contactJacobian.Friction0.Impulse = 0.0f;
                                contactJacobian.Friction1.Impulse = 0.0f;
                                contactJacobian.AngularFriction.Impulse = 0.0f;

                                // Calculate average position
                                float invNumContacts = math.rcp(contactJacobian.BaseJacobian.NumContacts);
                                centerA *= invNumContacts;
                                centerB *= invNumContacts;

                                // Choose friction axes
                                CalculatePerpendicularNormalized(contactJacobian.BaseJacobian.Normal, out float3 frictionDir0, out float3 frictionDir1);

                                // Build linear jacobian
                                float invEffectiveMass0, invEffectiveMass1;
                                {
                                    float3 armA = centerA;
                                    float3 armB = centerB;
                                    BuildJacobian(worldFromA, worldFromB, frictionDir0, armA, armB, velocityA.InverseInertia, velocityB.InverseInertia, sumInvMass,
                                        out contactJacobian.Friction0.AngularA, out contactJacobian.Friction0.AngularB, out invEffectiveMass0);
                                    BuildJacobian(worldFromA, worldFromB, frictionDir1, armA, armB, velocityA.InverseInertia, velocityB.InverseInertia, sumInvMass,
                                        out contactJacobian.Friction1.AngularA, out contactJacobian.Friction1.AngularB, out invEffectiveMass1);
                                }

                                // Build angular jacobian
                                float invEffectiveMassAngular;
                                {
                                    contactJacobian.AngularFriction.AngularA = math.mul(worldFromA.InverseRotation, contactJacobian.BaseJacobian.Normal);
                                    contactJacobian.AngularFriction.AngularB = math.mul(worldFromB.InverseRotation, -contactJacobian.BaseJacobian.Normal);
                                    float3 temp = contactJacobian.AngularFriction.AngularA * contactJacobian.AngularFriction.AngularA * velocityA.InverseInertia;
                                    temp += contactJacobian.AngularFriction.AngularB * contactJacobian.AngularFriction.AngularB * velocityB.InverseInertia;
                                    invEffectiveMassAngular = math.csum(temp);
                                }

                                // Build effective mass
                                {
                                    // Build the inverse effective mass matrix
                                    var invEffectiveMassDiag = new float3(invEffectiveMass0, invEffectiveMass1, invEffectiveMassAngular);
                                    var invEffectiveMassOffDiag = new float3( // (0, 1), (0, 2), (1, 2)
                                        JacobianUtilities.CalculateInvEffectiveMassOffDiag(contactJacobian.Friction0.AngularA, contactJacobian.Friction1.AngularA, velocityA.InverseInertia,
                                        contactJacobian.Friction0.AngularB, contactJacobian.Friction1.AngularB, velocityB.InverseInertia),
                                        JacobianUtilities.CalculateInvEffectiveMassOffDiag(contactJacobian.Friction0.AngularA, contactJacobian.AngularFriction.AngularA, velocityA.InverseInertia,
                                        contactJacobian.Friction0.AngularB, contactJacobian.AngularFriction.AngularB, velocityB.InverseInertia),
                                        JacobianUtilities.CalculateInvEffectiveMassOffDiag(contactJacobian.Friction1.AngularA, contactJacobian.AngularFriction.AngularA, velocityA.InverseInertia,
                                        contactJacobian.Friction1.AngularB, contactJacobian.AngularFriction.AngularB, velocityB.InverseInertia));

                                    // Invert the matrix and store it to the jacobians
                                    if (!JacobianUtilities.InvertSymmetricMatrix(invEffectiveMassDiag, invEffectiveMassOffDiag, out float3 effectiveMassDiag, out float3 effectiveMassOffDiag))
                                    {
                                        // invEffectiveMass can be singular if the bodies have infinite inertia about the normal.
                                        // In that case angular friction does nothing so we can regularize the matrix, set col2 = row2 = (0, 0, 1)
                                        invEffectiveMassOffDiag.y = 0.0f;
                                        invEffectiveMassOffDiag.z = 0.0f;
                                        invEffectiveMassDiag.z = 1.0f;
                                        bool success = JacobianUtilities.InvertSymmetricMatrix(invEffectiveMassDiag, invEffectiveMassOffDiag, out effectiveMassDiag, out effectiveMassOffDiag);
                                        Assert.IsTrue(success); // it should never fail, if it does then friction will be disabled
                                    }
                                    contactJacobian.Friction0.EffectiveMass = effectiveMassDiag.x;
                                    contactJacobian.Friction1.EffectiveMass = effectiveMassDiag.y;
                                    contactJacobian.AngularFriction.EffectiveMass = effectiveMassDiag.z;
                                    contactJacobian.FrictionEffectiveMassOffDiag = effectiveMassOffDiag;
                                }

                                // Reduce friction to 1/4 of the impulse if there will be restitution
                                if (applyRestitution)
                                {
                                    contactJacobian.Friction0.EffectiveMass *= 0.25f;
                                    contactJacobian.Friction1.EffectiveMass *= 0.25f;
                                    contactJacobian.AngularFriction.EffectiveMass *= 0.25f;
                                    contactJacobian.FrictionEffectiveMassOffDiag *= 0.25f;
                                }
                            }
                        }
                        // Much less data needed for triggers
                        else
                        {
                            ref TriggerJacobian triggerJacobian = ref jacobianHeader.AccessBaseJacobian<TriggerJacobian>();

                            triggerJacobian.BaseJacobian = baseJac;
                            triggerJacobian.ColliderKeys = contactHeader.ColliderKeys;

                            // Build normal jacobians
                            var centerA = new float3(0.0f);
                            var centerB = new float3(0.0f);
                            for (int j = 0; j < contactHeader.NumContacts; j++)
                            {
                                // Build the jacobian
                                BuildContactJacobian(
                                    j, triggerJacobian.BaseJacobian.Normal, worldFromA, worldFromB, invTimestep, velocityA, velocityB, sumInvMass, maxDepenetrationVelocity,
                                    ref jacobianHeader, ref centerA, ref centerB, ref contactReader);
                            }
                        }
                    }
                }
                else
                {
                    Joint joint = world.Joints[pair.JointIndex];
                    // Need to fetch the real body indices from the joint, as the scheduler may have reordered them
                    int bodyAIndex = joint.BodyPair.BodyAIndex;
                    int bodyBIndex = joint.BodyPair.BodyBIndex;

                    GetMotion(ref world, bodyAIndex, out MotionVelocity velocityA, out MotionData motionA);
                    GetMotion(ref world, bodyBIndex, out MotionVelocity velocityB, out MotionData motionB);

                    BuildJointJacobian((JointData*)joint.JointData.GetUnsafePtr(), joint.BodyPair, velocityA, velocityB,
                        motionA, motionB, timestep, numSolverIterations, ref jacobianWriter);
                }
            }

            contactReader.EndForEachIndex();
            jacobianWriter.EndForEachIndex();
        }

        internal static unsafe void BuildJointJacobian([NoAlias] JointData* jointData, BodyIndexPair pair,
            MotionVelocity velocityA, MotionVelocity velocityB, MotionData motionA, MotionData motionB,
            float timestep, int numIterations, [NoAlias] ref NativeStream.Writer jacobianWriter)
        {
            var bodyAFromMotionA = new MTransform(motionA.BodyFromMotion);
            MTransform motionAFromJoint = Mul(Inverse(bodyAFromMotionA), jointData->AFromJoint);

            var bodyBFromMotionB = new MTransform(motionB.BodyFromMotion);
            MTransform motionBFromJoint = Mul(Inverse(bodyBFromMotionB), jointData->BFromJoint);

            foreach( Constraint constraint in jointData->Constraints)
            {
                JacobianType jacType;
                switch (constraint.Type)
                {
                    case ConstraintType.Linear:
                        jacType = JacobianType.LinearLimit;
                        break;
                    case ConstraintType.Angular:
                        switch (constraint.Dimension)
                        {
                            case 1:
                                jacType = JacobianType.AngularLimit1D;
                                break;
                            case 2:
                                jacType = JacobianType.AngularLimit2D;
                                break;
                            case 3:
                                jacType = JacobianType.AngularLimit3D;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // Write size before every jacobian
                JacobianFlags jacFlags = 0;
                int jacobianSize = JacobianHeader.CalculateSize(jacType, jacFlags);
                jacobianWriter.Write(jacobianSize);

                // Allocate all necessary data for this jacobian
                byte* jacobianPtr = jacobianWriter.Allocate(jacobianSize);
#if DEVELOPMENT_BUILD
                if (((long)jacobianPtr & 0x3) != 0)
                    throw new InvalidOperationException("Jacobians must be 4 byte aligned");
#endif
                ref JacobianHeader header = ref UnsafeUtilityEx.AsRef<JacobianHeader>(jacobianPtr);
                header.BodyPair = pair;
                header.Type = jacType;
                header.Flags = jacFlags;

                JacobianUtilities.CalculateTauAndDamping(constraint, timestep, numIterations, out float tau, out float damping);

                // Build the Jacobian
                switch (constraint.Type)
                {
                    case ConstraintType.Linear:
                        header.AccessBaseJacobian<LinearLimitJacobian>().Build(
                            motionAFromJoint, motionBFromJoint,
                            velocityA, velocityB, motionA, motionB, constraint, tau, damping);
                        break;
                    case ConstraintType.Angular:
                        switch (constraint.Dimension)
                        {
                            case 1:
                                header.AccessBaseJacobian<AngularLimit1DJacobian>().Build(
                                    motionAFromJoint, motionBFromJoint,
                                    velocityA, velocityB, motionA, motionB, constraint, tau, damping);
                                break;
                            case 2:
                                header.AccessBaseJacobian<AngularLimit2DJacobian>().Build(
                                    motionAFromJoint, motionBFromJoint,
                                    velocityA, velocityB, motionA, motionB, constraint, tau, damping);
                                break;
                            case 3:
                                header.AccessBaseJacobian<AngularLimit3DJacobian>().Build(
                                    motionAFromJoint, motionBFromJoint,
                                    velocityA, velocityB, motionA, motionB, constraint, tau, damping);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static void Solve(
            NativeSlice<MotionVelocity> motionVelocities,
            [NoAlias] ref NativeStream.Reader jacobianReader,
            [NoAlias] ref NativeStream.Writer collisionEventsWriter,
            [NoAlias] ref NativeStream.Writer triggerEventsWriter,
            int workItemIndex,
            StepInput stepInput)
        {
            if (stepInput.IsLastIteration)
            {
                collisionEventsWriter.BeginForEachIndex(workItemIndex);
                triggerEventsWriter.BeginForEachIndex(workItemIndex);
            }

            var jacIterator = new JacobianIterator(jacobianReader, workItemIndex);
            while (jacIterator.HasJacobiansLeft())
            {
                ref JacobianHeader header = ref jacIterator.ReadJacobianHeader();

                // Static-static pairs should have been filtered during broadphase overlap test
                Assert.IsTrue(header.BodyPair.BodyAIndex < motionVelocities.Length || header.BodyPair.BodyBIndex < motionVelocities.Length);

                // Get the motion pair
                MotionVelocity velocityA = header.BodyPair.BodyAIndex < motionVelocities.Length ? motionVelocities[header.BodyPair.BodyAIndex] : MotionVelocity.Zero;
                MotionVelocity velocityB = header.BodyPair.BodyBIndex < motionVelocities.Length ? motionVelocities[header.BodyPair.BodyBIndex] : MotionVelocity.Zero;

                // Solve the jacobian
                header.Solve(ref velocityA, ref velocityB, stepInput, ref collisionEventsWriter, ref triggerEventsWriter);

                // Write back velocity for dynamic bodies
                if (header.BodyPair.BodyAIndex < motionVelocities.Length)
                {
                    motionVelocities[header.BodyPair.BodyAIndex] = velocityA;
                }
                if (header.BodyPair.BodyBIndex < motionVelocities.Length)
                {
                    motionVelocities[header.BodyPair.BodyBIndex] = velocityB;
                }
            }

            if (stepInput.IsLastIteration)
            {
                collisionEventsWriter.EndForEachIndex();
                triggerEventsWriter.EndForEachIndex();
            }
        }

        #endregion
    }
}