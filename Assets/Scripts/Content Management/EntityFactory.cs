// EntityFactory is a static class that holds definitions for instantiation of each and every Entity.
// A library of generic entity and component constructors are included here to help simplify entity construction.
// To add a new EntityID: Add an enum entry and a switch case.

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

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

            case EntityID.ID.CharacterGoblin: ConstructCharacter(e, pos, Resource.Mat.GoblinGreen, 125, 0); break;
            case EntityID.ID.CharacterPlayer: ConstructCharacter(e, pos, Resource.Mat.HumanBald, 150, 1); break;

            case EntityID.ID.ItemApple:
                AddTransform(e, pos);
                AddColliderCapsule(e, 0.3f, 0.5f, 0.3f, false, true);
                AddBillboard(e, Resource.Mat.Apple, false);
                AddAnimation(e, UVAnimation.Anim.Character_Walk, 5f);
                break;

            case EntityID.ID.TerrainGrass: ConstructTerrain(e, pos, Resource.Mat.Grass); break;

            default: break;
        }

        return e;
    }

    #region Generic Entity Constructors

    private static void ConstructCharacter(Entity e, float3 pos, Resource.Mat material, float speed, int playerID)
    {
        AddTransform(e, pos);
        AddColliderCapsule(e, 0.3f, 0.5f, 0.3f, true, false);

        AddMovement(e, speed);
        m.AddComponentData(e, new C_ControlsMovement());

        AddBillboard(e, material, true);
        AddAnimation(e, UVAnimation.Anim.Character_Stand, 10f);

        m.AddComponentData(e, new C_Jumps { power = 4f } );

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

    private static void ConstructTerrain(Entity e, float3 pos, Resource.Mat material)
    {
        AddTransform(e, pos);
        AddColliderBox(e, 1f, 0.5f, 1f, 0f, 0.25f, 0f, false, false);
        AddMesh(e, Resource.Mesh.Slab, material);
    }

    #endregion

    #region Component Constructors

    private static void AddTransform(Entity e, float3 pos)
    {
        m.AddComponentData(e, new LocalToWorld());
        m.AddComponentData(e, new Translation { Value = pos });
        m.AddComponentData(e, new Rotation { Value = quaternion.identity });
        m.AddComponentData(e, new C_FacesDirection { dirFacing = new float3(1f, 0f, 0f) });
    }

    private static void AddParent(Entity e, Entity parent)
    {
        m.AddComponentData(e, new Parent { Value = parent });
        m.AddComponentData(e, new LocalToParent());
    }

    private static void AddMovement(Entity e, float speed)
    {
        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new C_Moves
        {
            speed = speed,
        });
    }

    private static void AddMesh(Entity e, Resource.Mesh mesh, Resource.Mat material)
    {
        m.AddComponentData(e, new C_RendersMesh
        {
            mesh = mesh,
            material = material,
            UV = new float4(1f, 1f, 0f, 0f)
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

    private static void AddAnimation(Entity e, UVAnimation.Anim anim, float animSpeed)
    {
        m.AddComponentData(e, new C_Animates
        {
            currentAnim = anim,
            animSpeed = animSpeed
        });
    }

    private static void AddColliderBox(Entity e, float sizeX, float sizeY, float sizeZ, float centerX, float centerY, float centerZ, bool dynamic, bool isTrigger)
    {
        Material physMat = Material.Default;
        if (isTrigger)
        {
            physMat.Flags = Material.MaterialFlags.IsTrigger;
        }

        BoxGeometry boxGeo = new BoxGeometry
        {
            Center = new float3(centerX, centerY, centerZ),
            Orientation = quaternion.identity,
            Size = new float3(sizeX, sizeY, sizeZ),
            BevelRadius = 0.05f
        };
        PhysicsCollider collider = new PhysicsCollider { Value = BoxCollider.Create(boxGeo, CollisionFilter.Default, physMat) };

        m.AddComponentData(e, collider);

        if (dynamic)
        {
            AddMass(e, collider);
        }
    }

    private static void AddColliderCapsule(Entity e, float bottomPoint, float topPoint, float radius, bool dynamic, bool isTrigger)
    {
        Material physMat = Material.Default;
        if (isTrigger)
        {
            physMat.Flags = Material.MaterialFlags.IsTrigger;
        }

        CapsuleGeometry capsuleGeo = new CapsuleGeometry
        {
            Vertex0 = new float3(0f, bottomPoint, 0f),
            Vertex1 = new float3(0f, topPoint, 0f),
            Radius = radius
        };
        PhysicsCollider collider = new PhysicsCollider { Value = CapsuleCollider.Create(capsuleGeo, CollisionFilter.Default, physMat) };

        m.AddComponentData(e, collider);

        if (dynamic)
        {
            AddMass(e, collider);
        }
    }

    private static void AddMass(Entity e, PhysicsCollider collider)
    {
        PhysicsMass physicsMass = PhysicsMass.CreateDynamic(collider.MassProperties, 1f);
        physicsMass.InverseInertia = new float3(0f, 0f, 0f); // Freeze rotation
        m.AddComponentData(e, physicsMass);

        PhysicsGravityFactor gravity = new PhysicsGravityFactor();
        gravity.Value = 1f;
        m.AddComponentData(e, gravity);
    }

    #endregion
}