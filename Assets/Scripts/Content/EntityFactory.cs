// EntityFactory is a static class that holds definitions for instantiation of each and every Entity.
// A library of generic entity and component constructors are included here to help simplify entity construction.
// To add a new EntityID: Add an enum entry and a switch case.

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
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
        TerrainGrass,
    }
}

public static class EntityFactory
{
    private static EntityManager m = World.DefaultGameObjectInjectionWorld.EntityManager;

    public static Entity Instantiate(EntityID.ID id, float3 pos)
    {
        Entity e = m.CreateEntity();
        m.SetName(e, id.ToString());

        switch (id)
        {
            case EntityID.ID._Empty: break;

            case EntityID.ID.CharacterGoblin: ConstructCharacter(e, pos, Resource.Mat.GoblinGreen, 125f, 0); break;
            case EntityID.ID.CharacterPlayer: ConstructCharacter(e, pos, Resource.Mat.HumanBald, 150f, 1); break;
            case EntityID.ID.ItemApple: ConstructItem(e, pos, Resource.Mat.Apple); break;
            case EntityID.ID.TerrainGrass: ConstructTerrain(e, id, pos, Resource.Mat.Grass); break;

            default: break;
        }

        return e;
    }

    #region Collision Filters
    private enum CollisionLayer
    {
        Solid = 1 << 0,
        Character = 1 << 1,
        DamageTrigger = 1 << 2,
        Item = 1 << 3,
        ItemTrigger = 1 << 4
    }

    private static CollisionFilter
        filterSolid = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Solid),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Item)
        },
        filterCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Character),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.ItemTrigger | CollisionLayer.Character | CollisionLayer.DamageTrigger)
        },
        filterDamageTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.DamageTrigger),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Item)
        },
        filterItem = new CollisionFilter()
        {
            BelongsTo =     (uint)(CollisionLayer.Item),
            CollidesWith =  (uint)(CollisionLayer.Solid | CollisionLayer.Item | CollisionLayer.DamageTrigger)
        },
        filterItemTrigger = new CollisionFilter()
        {
            BelongsTo =     (uint)(CollisionLayer.ItemTrigger),
            CollidesWith =  (uint)(CollisionLayer.Character)
        };
    #endregion

    #region Generic Entity Constructors

    private static void ConstructCharacter(Entity e, float3 pos, Resource.Mat material, float speed, int playerID)
    {
        AddTransform(e, pos + new float3(0f, 0.15f, 0f));

        //PhysicsCollider collider = CreateColliderCapsule(0.3f, 0.5f, 0.3f, filterCharacter, CreatePhysicsMaterial(0f, 0f, false));
        PhysicsCollider collider = CreateMeshCollider(Resource.Mesh.Cylinder, new float3(0.6f, 1f, 0.6f), filterCharacter, CreatePhysicsMaterial(0f, 0f, false));
        m.AddComponentData(e, collider);
        AddMass(e, collider);

        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new C_ControlsMovement { speed = speed });
        m.AddComponentData(e, new C_CanJump { power = 400f });
        m.AddComponentData(e, new C_HasVerticalMovement() );
        m.AddComponentData(e, new C_HasGravity());

        m.AddComponentData(e, new C_CanHold());
        m.AddComponentData(e, new C_CanAttack());

        m.AddComponentData(e, new C_TakesDamage { health = 2f });
        m.AddComponentData(e, new C_CanDie { deathTimer = 0.15f });
        m.AddComponentData(e, new C_ReactsToForce { forceResistanceFactor = 1f });

        AddBillboard(e, material, true);
        AddAnimation(e, UVAnimation.Anim.Character_Stand, 10f, false);

        if (playerID > 0)
        {
            m.AddComponentData(e, new C_IsControlledByPlayer { playerID = playerID });
            m.AddComponentData(e, new C_IsFollowedByCamera());
        }
        else
        {
            m.AddComponentData(e, new C_IsControlledByAI_Wander
            {
                pauseTimeRange = new float2(1f, 2f),
                moveTimeRange = new float2(2f, 3f)
            });
        }
    }

    private static void ConstructItem(Entity e, float3 pos, Resource.Mat material)
    {
        AddTransform(e, pos);
        AddBillboard(e, material, false);
        AddAnimation(e, UVAnimation.Anim.Character_Walk, 5f, false);

        m.AddComponentData(e, new C_CanBeHeld());
        m.AddComponentData(e, new C_ReactsToForce { forceResistanceFactor = 0.5f });

        //PhysicsCollider triggerCollider  = CreateColliderCapsule(0.3f, 0.3f, 0.5f, filterItemTrigger, CreatePhysicsMaterial(0f, 0f, true));
        //PhysicsCollider itemBodyCollider = CreateColliderCapsule(0.3f, 0.3f, 0.3f, filterItem, CreatePhysicsMaterial(0.3f, 0f, false)); // Friction no longer matters..
        PhysicsCollider triggerCollider  = CreateMeshCollider(Resource.Mesh.Cylinder, 1f, filterItemTrigger, CreatePhysicsMaterial(0f, 0f, true));
        PhysicsCollider itemBodyCollider = CreateMeshCollider(Resource.Mesh.Cylinder, 0.5f, filterItem, CreatePhysicsMaterial(0f, 0f, false));
        m.AddComponentData(e, CreateCompoundCollider(triggerCollider, itemBodyCollider));

        AddMass(e, itemBodyCollider);
        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new C_HasVerticalMovement());
        m.AddComponentData(e, new C_HasGravity());
    }

    private static void ConstructTerrain(Entity e, EntityID.ID id, float3 pos, Resource.Mat material)
    {
        AddTransform(e, pos);

        // FIXME: Position in Leveldata should be set directly from Level instead of from world space instantiation position.
        m.AddComponentData(e, new C_LevelData { id = id, x = (int)pos.x, y = (int)(pos.y * 2), z = (int)pos.z });
        m.AddComponentData(e, new C_ConnectsToNeighbors());
        m.AddComponentData(e, new C_IsAwaitingTerrainMeshRefresh());

        AddMesh(e, Resource.Mesh.Slab, material);
        m.AddComponentData(e, CreateBoxCollider(1f, 0.5f, 1f, 0f, 0.25f, 0f, filterSolid, CreatePhysicsMaterial(0f, 0f, false))); 
    }

    #endregion

    #region Component Constructors

    private static void AddTransform(Entity e, float3 pos)
    {
        m.AddComponentData(e, new LocalToWorld());
        m.AddComponentData(e, new Translation { Value = pos });
        m.AddComponentData(e, new Rotation { Value = quaternion.identity });
        m.AddComponentData(e, new C_FacesDirection { normalizedLateralDir = new float3(1f, 0f, 0f) });
    }

    //private static void AddParent(Entity e, Entity parent)
    //{
        //m.AddComponentData(e, new Parent { Value = parent });
        //m.AddComponentData(e, new LocalToParent());
    //}

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
        AddMesh(e, Resource.Mesh.BillboardQuad, material);
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
            //BevelRadius = 0.05f
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

    #region Effects

    // TODO: How to better handle instantiating effects?
    public static Entity InstantiateEffectSwipe(Entity originator, float3 pos, quaternion rotation)
    {
        Entity e = m.CreateEntity();
        m.SetName(e, "EffectSwipe");

        AddTransform(e, pos);
        m.SetComponentData(e, new Rotation { Value = rotation });
        AddMesh(e, Resource.Mesh.BillboardQuad, Resource.Mat.EffectSwipe);

        m.AddComponentData(e, CreateBoxCollider(1f, 1f, 0.1f, 0f, 0.5f, 0f, filterDamageTrigger, CreatePhysicsMaterial(0f, 0f, true)));
        m.AddComponentData(e, new C_InflictsDamage { damage = 1f, originator = originator, invulnerableTime = 0.06f } );
        m.AddComponentData(e, new C_AppliesForce { force = 1000f, originator = originator, forceTime = 0.15f });
        m.AddComponentData(e, new C_IsDestroyedAfterTimer { timer = 0.05f });

        return e;
    }

    public static Entity InstantiateEffectDeath(Entity dyingEntity)
    {
        Entity e = m.CreateEntity();
        m.SetName(e, "EffectDeath");

        AddTransform(e, float3.zero);
        m.AddComponentData(e, new C_SnapsToEntityPosition { entityToSnapTo = dyingEntity, offset = new float3(0f, -0.4f, 0f) });
        AddBillboard(e, Resource.Mat.EffectDeath, false);
        AddAnimation(e, UVAnimation.Anim.Effect_Death, 18f, true);

        return e;
    }

    #endregion
}