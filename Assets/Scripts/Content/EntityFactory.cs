// EntityFactory is a static class that holds definitions for instantiation of each and every Entity.
// A library of generic entity and component constructors are included here to help simplify entity construction.
// To add a new EntityID: Add an enum entry and a switch case.

// TODO: Convert to blueprint / element / dependency system

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Color = UnityEngine.Color;
using PhysicsMaterial = Unity.Physics.Material;

public static class EntityID
{
    public enum ID
    {
        _Empty,
        CharacterGoblin,
        CharacterPlayer,
        ItemApple,
        ItemTurnip,
        TerrainGrass,
        Tree
    }
}

public static class EntityFactory
{
    private static EntityManager m = World.DefaultGameObjectInjectionWorld.EntityManager;

    public static Entity Instantiate(EntityID.ID id, float3 pos)
    {
        Entity e = m.CreateEntity();
        //m.SetName(e, id.ToString());

        switch (id)
        {
            case EntityID.ID._Empty: break;

            case EntityID.ID.CharacterGoblin: ConstructCharacter(e, pos, Resource.Mat.GoblinGreen, 125f, 0); break;
            case EntityID.ID.CharacterPlayer: ConstructCharacter(e, pos, Resource.Mat.HumanBald, 150f, 1); break;
            case EntityID.ID.ItemApple: ConstructItem(e, pos, Resource.Mat.Apple); break;
            case EntityID.ID.ItemTurnip: ConstructItem(e, pos, Resource.Mat.Turnip); break;
            case EntityID.ID.TerrainGrass: ConstructTerrain(e, id, pos, Resource.Mat.Grass); break;
            case EntityID.ID.Tree: ConstructTree(e, id, pos); break;

            default: break;
        }

        return e;
    }

    #region Collision Filters
    private enum CollisionLayer
    {
        SolidEnvironment = 1 << 0,
        SolidBodyCharacter = 1 << 1,
        SolidBodyItem = 1 << 2,
        DamageTrigger = 1 << 3,
        ScanTrigger = 1 << 4,
        BodyTrigger = 1 << 5
    }

    private static CollisionFilter
        filterSolidEnvironment = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.SolidEnvironment),
            CollidesWith = (uint)(CollisionLayer.SolidBodyCharacter | CollisionLayer.SolidBodyItem)
        },
        filterSolidBodyCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.SolidBodyCharacter),
            CollidesWith = (uint)(CollisionLayer.SolidEnvironment | CollisionLayer.BodyTrigger | CollisionLayer.SolidBodyCharacter | CollisionLayer.DamageTrigger)
        },
        filterSolidBodyItem = new CollisionFilter()
        {
            BelongsTo =     (uint)(CollisionLayer.SolidBodyItem),
            CollidesWith =  (uint)(CollisionLayer.SolidEnvironment | CollisionLayer.SolidBodyItem | CollisionLayer.DamageTrigger)
        },
        filterDamageTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.DamageTrigger),
            CollidesWith = (uint)(CollisionLayer.SolidBodyCharacter | CollisionLayer.SolidBodyItem)
        },
        filterScanTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.ScanTrigger),
            CollidesWith = (uint)(CollisionLayer.BodyTrigger)
        },
        filterBodyTrigger = new CollisionFilter()
        {
            BelongsTo =     (uint)(CollisionLayer.BodyTrigger),
            CollidesWith =  (uint)(CollisionLayer.SolidBodyCharacter | CollisionLayer.ScanTrigger)
        };
    #endregion

    #region Generic Entity Constructors

    private static void ConstructCharacter(Entity e, float3 pos, Resource.Mat material, float speed, int playerID)
    {
        AddTransform(e, pos + new float3(0f, 0.15f, 0f));

        // Graphics and Animation
        AddBillboard(e, material, true);
        AddAnimation(e, UVAnimation.Anim.Character_Stand, 10f, false);
        DynamicBuffer<C_BufferElement_EntityAnimation> dynamicBuffer = m.AddBuffer<C_BufferElement_EntityAnimation>(e);
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Attack,   speed = 8f, priority = 40, determiner = AnimationStateDeterminer.IsAttacking });
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Fall,     speed = 8f, priority = 20, determiner = AnimationStateDeterminer.IsFalling });
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Hurt,     speed = 8f, priority = 30, determiner = AnimationStateDeterminer.IsHurt });
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Jump,     speed = 8f, priority = 21, determiner = AnimationStateDeterminer.IsRising });
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Stand,    speed = 6f, priority = 00, determiner = AnimationStateDeterminer.Default });
        dynamicBuffer.Add(new C_BufferElement_EntityAnimation { anim = UVAnimation.Anim.Character_Walk,     speed = 8f, priority = 10, determiner = AnimationStateDeterminer.IsControllingLateralMovement });

        // Colliders
        PhysicsCollider triggerCollider = CreateMeshCollider(Resource.Mesh.Cylinder, 1f, filterBodyTrigger, CreatePhysicsMaterial(0f, 0f, true));
        PhysicsCollider bodyCollider = CreateMeshCollider(Resource.Mesh.Cylinder, new float3(0.5f, 1f, 0.5f), filterSolidBodyCharacter, CreatePhysicsMaterial(0f, 0f, false));
        m.AddComponentData(e, CreateCompoundCollider(triggerCollider, bodyCollider));
        AddMass(e, bodyCollider);

        // Movement
        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new C_Ability_CanControlLateralMovement { speed = speed });
        m.AddComponentData(e, new C_Ability_CanJump { power = 400f });
        m.AddComponentData(e, new C_Ability_CanMoveVertically() );
        m.AddComponentData(e, new C_Ability_AffectedByGravity());

        // Abilities
        m.AddComponentData(e, new C_Ability_CanHold());
        m.AddComponentData(e, new C_Ability_CanAttack());
        m.AddComponentData(e, new C_Ability_CanDie { deathTimer = 0.15f });
        m.AddComponentData(e, new C_Ability_CanReactToForce { forceResistanceFactor = 1f });

        // Control
        if (playerID > 0)
        {
            m.AddComponentData(e, new C_Controller_Player { playerID = playerID });
            m.AddComponentData(e, new C_IsFollowedByCamera());
            m.AddComponentData(e, new C_Ability_CanTakeDamage { health = 50f });
            m.AddComponentData(e, new C_Ability_CanBeScannedByAI());
        }
        else
        {
            m.AddComponentData(e, new C_Controller_AI { idleBehavior = AIBehavior.Wander, reactionBehavior = AIBehavior.ChaseAndAttackEntity });
            m.AddComponentData(e, new C_Ability_CanTakeDamage { health = 3f });
            InstantiateScanEntity(e);
        }
    }

    private static void ConstructItem(Entity e, float3 pos, Resource.Mat material)
    {
        AddTransform(e, pos);
        AddBillboard(e, material, false);
        AddAnimation(e, UVAnimation.Anim.Character_Walk, 5f, false);

        m.AddComponentData(e, new C_Ability_CanBeHeld());
        m.AddComponentData(e, new C_Ability_CanReactToForce { forceResistanceFactor = 0.5f });

        PhysicsCollider triggerCollider  = CreateMeshCollider(Resource.Mesh.Cylinder, 1f, filterBodyTrigger, CreatePhysicsMaterial(0f, 0f, true));
        PhysicsCollider itemBodyCollider = CreateMeshCollider(Resource.Mesh.Cylinder, 0.5f, filterSolidBodyItem, CreatePhysicsMaterial(0f, 0f, false));
        m.AddComponentData(e, CreateCompoundCollider(triggerCollider, itemBodyCollider));

        AddMass(e, itemBodyCollider);
        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new C_Ability_CanMoveVertically());
        m.AddComponentData(e, new C_Ability_AffectedByGravity());
    }

    private static void ConstructTerrain(Entity e, EntityID.ID id, float3 pos, Resource.Mat material)
    {
        AddTransform(e, pos);

        // FIXME: Position in Leveldata should be set directly from Level instead of from world space instantiation position.
        m.AddComponentData(e, new C_LevelData { id = id, x = (int)pos.x, y = (int)(pos.y * 2), z = (int)pos.z });
        m.AddComponentData(e, new C_Ability_CanConnectMeshToNeighbors());
        m.AddComponentData(e, new C_State_IsAwaitingNeighborMeshRefresh());

        AddMesh(e, Resource.Mesh.Slab, material);
        m.AddComponentData(e, CreateBoxCollider(1f, 0.5f, 1f, 0f, 0.25f, 0f, filterSolidEnvironment, CreatePhysicsMaterial(0f, 0f, false))); 
    }

    private static void ConstructTree(Entity e, EntityID.ID id, float3 pos)
    {
        AddTransform(e, pos);

        m.AddComponentData(e, new C_LevelData { id = id, x = (int)pos.x, y = (int)(pos.y * 2), z = (int)pos.z });

        AddMesh(e, Resource.Mesh.Tree, Resource.Mat.Tree);
        m.AddComponentData(e, CreateMeshCollider(Resource.Mesh.Cylinder, new float3(3f, 3f, 3f), filterSolidEnvironment, CreatePhysicsMaterial(0f, 0f, false)));
    }

    #endregion

    #region Component Constructors

    private static void AddTransform(Entity e, float3 pos)
    {
        m.AddComponentData(e, new LocalToWorld());
        m.AddComponentData(e, new Translation { Value = pos });
        m.AddComponentData(e, new Rotation { Value = quaternion.identity });
        m.AddComponentData(e, new C_Ability_FacesDirection { normalizedLateralDir = new float3(1f, 0f, 0f) });
    }

    private static void AddMesh(Entity e, Resource.Mesh mesh, Resource.Mat material)
    {
        m.AddComponentData(e, new C_HasMesh
        {
            mesh = mesh,
            material = material,
            UV = new float4(1f, 1f, 0f, 0f),
            color = Color.white
        });
    }

    private static void AddBillboard(Entity e, Resource.Mat material, bool flipTextureFacing)
    {
        AddMesh(e, Resource.Mesh.QuadBillboard, material);
        m.AddComponentData(e, new C_IsBillboarded
        {
            flipTexureFacing = flipTextureFacing,
            xScale = 1f
        });
    }

    private static void AddAnimation(Entity e, UVAnimation.Anim anim, float animSpeed, bool destroyAfterAnim)
    {
        m.AddComponentData(e, new C_Animates
        {
            currentAnim = anim,
            animSpeed = animSpeed,
            destroyAfterAnim = destroyAfterAnim
        });
    }

    private static PhysicsCollider CreateBoxCollider(float sizeX, float sizeY, float sizeZ, float centerX, float centerY, float centerZ, CollisionFilter filter, PhysicsMaterial physicsMaterial)
    {
        BoxGeometry boxGeo = new BoxGeometry
        {
            Center = new float3(centerX, centerY, centerZ),
            Orientation = quaternion.identity,
            Size = new float3(sizeX, sizeY, sizeZ),
            BevelRadius = 0.05f
        };

        return new PhysicsCollider { Value = BoxCollider.Create(boxGeo, filter, physicsMaterial) };
    }

    private static PhysicsCollider CreateCapsuleCollider(float bottomPoint, float topPoint, float radius, CollisionFilter filter, PhysicsMaterial physicsMaterial)
    {
        CapsuleGeometry capsuleGeo = new CapsuleGeometry
        {
            Vertex0 = new float3(0f, bottomPoint, 0f),
            Vertex1 = new float3(0f, topPoint, 0f),
            Radius = radius
        };

        return new PhysicsCollider { Value = CapsuleCollider.Create(capsuleGeo, filter, physicsMaterial) };
    }

    private static PhysicsCollider CreateMeshCollider(Resource.Mesh mesh, float3 scale, CollisionFilter filter, PhysicsMaterial physicsMaterial)
    {
        UnityEngine.Vector3[] v3Verts = Resource.GetMesh(mesh).vertices;
        float3[] float3Verts = new float3[v3Verts.Length];
        for (int i = 0; i < v3Verts.Length; i++)
        {
            float3Verts[i] = v3Verts[i] * scale;
        }
        NativeArray<float3> nativeVerts = new NativeArray<float3>(float3Verts, Allocator.Temp);
        return new PhysicsCollider { Value = ConvexCollider.Create(nativeVerts, ConvexHullGenerationParameters.Default, filter, physicsMaterial) };
    }

    private static PhysicsCollider CreateCompoundCollider(PhysicsCollider c1, PhysicsCollider c2)
    {
        CompoundCollider.ColliderBlobInstance[] colliderArray = new CompoundCollider.ColliderBlobInstance[2];
        colliderArray[0] = new CompoundCollider.ColliderBlobInstance { Collider = c1.Value };
        colliderArray[1] = new CompoundCollider.ColliderBlobInstance { Collider = c2.Value };
        NativeArray<CompoundCollider.ColliderBlobInstance> colliderNativeArray = new NativeArray<CompoundCollider.ColliderBlobInstance>(colliderArray, Allocator.TempJob);
        PhysicsCollider compoundCollider = new PhysicsCollider { Value = CompoundCollider.Create(colliderNativeArray) };
        colliderNativeArray.Dispose();
        return compoundCollider;
    }

    private static PhysicsMaterial CreatePhysicsMaterial(float friction, float bounciness, bool isTrigger)
    {
        PhysicsMaterial physicsMaterial = PhysicsMaterial.Default;

        physicsMaterial.Friction = friction;
        physicsMaterial.FrictionCombinePolicy = PhysicsMaterial.CombinePolicy.Maximum;

        physicsMaterial.Restitution = bounciness;
        physicsMaterial.RestitutionCombinePolicy = PhysicsMaterial.CombinePolicy.Maximum;

        if (isTrigger)
        {
            physicsMaterial.Flags = PhysicsMaterial.MaterialFlags.IsTrigger;
        }

        return physicsMaterial;
    }

    private static void AddMass(Entity e, PhysicsCollider collider)
    {
        PhysicsMass physicsMass = PhysicsMass.CreateDynamic(collider.MassProperties, 1f);
        physicsMass.InverseInertia = new float3(0f, 0f, 0f); // Freeze rotation
        m.AddComponentData(e, physicsMass);
        m.AddComponentData(e, new PhysicsGravityFactor { Value = 0f }); // Disable built-in gravity on all dynamic entities
    }

    #endregion

    #region Effects / Misc

    // TODO: How to better handle instantiating effects?
    public static Entity InstantiateEffectSwipe(Entity originator, float3 pos, quaternion rotation)
    {
        Entity e = m.CreateEntity();
        //m.SetName(e, "EffectSwipe");

        AddTransform(e, pos);
        m.SetComponentData(e, new Rotation { Value = rotation });
        AddMesh(e, Resource.Mesh.QuadFlat, Resource.Mat.EffectSwipe);

        m.AddComponentData(e, CreateBoxCollider(1.1f, 0.25f, 1.1f, 0f, 0f, 0.5f, filterDamageTrigger, CreatePhysicsMaterial(0f, 0f, true)));
        m.AddComponentData(e, new C_Ability_CanInflictDamage { damage = 1f, originator = originator, invulnerableTime = 0.14f } );
        m.AddComponentData(e, new C_Ability_CanApplyForce { force = 1000f, originator = originator, forceTime = 0.15f });
        m.AddComponentData(e, new C_State_IsDestroyedAfterTimer { timer = 0.05f });

        return e;
    }

    public static Entity InstantiateEffectDeath(Entity dyingEntity)
    {
        Entity e = m.CreateEntity();
        //m.SetName(e, "EffectDeath");

        AddTransform(e, float3.zero);
        m.AddComponentData(e, new C_Ability_SnapsToEntityPosition { entityToSnapTo = dyingEntity, offset = new float3(0f, -0.4f, 0f) });
        AddBillboard(e, Resource.Mat.EffectDeath, false);
        AddAnimation(e, UVAnimation.Anim.Effect_Death, 18f, true);

        return e;
    }

    private static Entity InstantiateScanEntity(Entity parent)
    {
        Entity scanEntity = m.CreateEntity();
        //m.SetName(scanEntity, "ScanEntity");
        AddTransform(scanEntity, 0f);

        m.AddBuffer<C_BufferElement_ScannedEntity>(scanEntity);
        m.AddComponentData(scanEntity, new C_Ability_CanScanForEntities());
        m.AddComponentData(scanEntity, CreateCapsuleCollider(0f, 0f, 6f, filterScanTrigger, CreatePhysicsMaterial(0f, 0f, true)));
        m.AddComponentData(scanEntity, new C_Ability_SnapsToEntityPosition { entityToSnapTo = parent, destroyOnParentLoss = true });

        return scanEntity;
    }

    #endregion
}