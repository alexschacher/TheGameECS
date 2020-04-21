// EntityFactory is a static class that the game uses to instantiate all entities.
// Entity definitions are all centralized and referenced from here.
// Entities may be further customized in game code once they are created by setting or adding component data.
// A library of generic entity and component constructors are written here to help simplify new entity definition.

// TODO: Split this class into two classes: public definitions of entities, and generic entity and component constructors.

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics;
using UnityEngine.Rendering;

public static class EntityFactory
{
    private static EntityManager m = World.DefaultGameObjectInjectionWorld.EntityManager;

    // Private component constructors:

    // Positional components

    private static void AddTransform(Entity e, float3 pos)
    {
        m.AddComponentData(e, new LocalToWorld());
        m.AddComponentData(e, new Translation { Value = pos });
        m.AddComponentData(e, new Rotation { Value = quaternion.identity });
    }

    private static void AddParent(Entity e, Entity parent)
    {
        m.AddComponentData(e, new Parent { Value = parent });
        m.AddComponentData(e, new LocalToParent());
    }

    private static void AddMovement(Entity e, float speed)
    {
        m.AddComponentData(e, new PhysicsVelocity());
        m.AddComponentData(e, new CompMoves {
            speed = speed,
            facingDir = new float3(1f,0f,0f) // Default facing direction
        });
    }

    // Graphical components
    
    private static void AddMesh(Entity e, Resource.Mesh mesh, Resource.Mat material)
    {
        m.AddComponentData(e, new CompRenderMesh
        {
            mesh = mesh,
            material = material,
            UV = new float4(1f, 1f, 0f, 0f)
        });
    }

    private static void AddMesh_Old(Entity e, Resource.Mesh mesh, Resource.Mat material)
    {
        // Old built in RenderMesh - material is shared, so setting UV doesnt work
        m.AddSharedComponentData(e, new RenderMesh
        {
            mesh = Resource.GetMesh(mesh),
            material = Resource.GetMat(material),
            subMesh = 0,
            castShadows = ShadowCastingMode.On,
            receiveShadows = true
        });
    } // <-- Delete this 

    private static Entity AddBillboard(Entity e, Resource.Mat material)
    {
        Entity childE = m.CreateEntity();
        m.SetName(childE, m.GetName(e) + "_Billboard");
        AddParent(childE, e);

        AddTransform(childE, float3.zero);
        m.AddComponentData(childE, new NonUniformScale());

        AddMesh(childE, Resource.Mesh.BillboardQuad, material);
        m.AddComponentData(childE, new CompBillboard());

        return childE;
    }

    private static void AddAnimation(Entity e, UVAnimation.Anim anim, float animSpeed)
    {
        m.AddComponentData(e, new CompAnimatedUV
        {
            currentAnim = anim,
            animSpeed = animSpeed
        });
    }

    // Physics components

    private static void AddColliderBox(Entity e, float sizeX, float sizeY, float sizeZ, float centerX, float centerY, float centerZ, bool dynamic)
    {
        PhysicsCollider collider = new PhysicsCollider
        {
            Value = BoxCollider.Create(new BoxGeometry
            {
                Center = new float3(centerX, centerY, centerZ),
                Orientation = quaternion.identity,
                Size = new float3(sizeX, sizeY, sizeZ),
                BevelRadius = 0.05f
            })
        };
        m.AddComponentData(e, collider);

        if (dynamic)
        {
            AddMass(e, collider);
        }
    }

    private static void AddColliderCapsule(Entity e,  bool dynamic)
    {
        PhysicsCollider collider = new PhysicsCollider
        {
            Value = CapsuleCollider.Create(new CapsuleGeometry
            {
                Vertex0 = new float3(0f, 0.5f, 0f),
                Vertex1 = new float3(0f, 0.5f, 0f),
                Radius = 0.5f
            })
        };
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
    }

    // Private generic entity constructors

    private static Entity Construct_Generic_Terrain(string name, float3 pos, Resource.Mat material)
    {
        Entity e = m.CreateEntity();
        m.SetName(e, name);

        AddTransform(e, pos);
        AddColliderBox(e, 1f, 0.5f, 1f, 0f, 0.25f, 0f, false);
        AddMesh(e, Resource.Mesh.Slab, material);

        return e;
    }

    private static Entity Construct_Generic_Character(string name, Resource.Mat material, float3 pos, float speed)
    {
        Entity e = m.CreateEntity();
        m.SetName(e, name);

        AddTransform(e, pos);
        AddColliderCapsule(e, true);
        AddMovement(e, speed);
        m.AddComponentData(e, new CompControlsMovement());

        Entity childE = AddBillboard(e, material);
        AddAnimation(childE, UVAnimation.Anim.Character_Stand, 10f);
        
        return e;
    }

    // Public specific entity constructors

    public static Entity Instantiate_Terrain_Grass(float3 pos)
    {
        Entity e = Construct_Generic_Terrain("Terrain_Grass", pos, Resource.Mat.Grass);
        return e;
    }

    public static Entity Instantiate_Terrain_Brick(float3 pos)
    {
        Entity e = Construct_Generic_Terrain("Terrain_Brick", pos, Resource.Mat.Brick);
        return e;
    }

    public static Entity Instantiate_Character_Player(float3 pos)
    {
        Entity e = Construct_Generic_Character("Character_Player", Resource.Mat.GoblinGreen, pos, 300);
        
        m.AddComponentData(e, new CompPlayerControlsMovement());
        m.AddComponentData(e, new CompCameraFollows());
        return e;
    }

    public static Entity Instantiate_Character_NPC(float3 pos)
    {
        Entity e = Construct_Generic_Character("Character_NPC", Resource.Mat.GoblinBlue, pos, 200);
        m.AddComponentData(e, new Comp_AIBehavior_Wander
        {
            pauseTimeRange = new float2(1f, 2f),
            moveTimeRange = new float2(2f, 3f)
        });
        return e;
    }
}