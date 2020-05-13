using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// Todo:

// Set floor bitmask seperate from cliff bitmask:
// When determining floor bitmask, check for exact neighbor ID match
// When determining terrain bitmask, check for if neighbor ID is >= first terrain ID definition (ex EntityID.ID.Grass) and <= last terrain ID definition (ex EntityID.ID.Snow)

// Figure out cliff sides: middle, top only, bottom only, topandbottom... a LOT of bitmasking. Can we make it a separate bitmask check somehow? 

public class S_RefreshTerrainMeshes : ComponentSystem
{
    const int s = 1, w = 2, e = 4, n = 8, t = 16, sb = 32, wb = 64, eb = 128, nb = 256;
    int bitmask, x, y, z;

    protected override void OnUpdate()
    {
        if (Level._instance == null)
        {
            Log.LogError("S_RefreshTerrainNeighbors: Level _instance is null.");
            return;
        }

        Entities.ForEach((
            Entity entity,
            ref C_LevelData cLevelData,
            ref C_HasMesh cMesh,
            ref Rotation cRotation,
            ref C_ConnectsToNeighbors cConnects,
            ref C_IsAwaitingTerrainMeshRefresh cAwaits) =>
        {
            PostUpdateCommands.RemoveComponent<C_IsAwaitingTerrainMeshRefresh>(entity);

            x = cLevelData.x;
            y = cLevelData.y;
            z = cLevelData.z;
            bitmask = 0;

            // Code commented out until tops and bottoms are implemented. Code should work, just need models created and mesh/rotation set correctly in switch statement
            //if (Level._instance.GetIfInRangeElseReturnInstead(x, y + 1, z, EntityID.ID._Empty) != cLevelData.id)            bitmask += t;
            if (Level._instance.GetIfInRangeElseReturnInstead(x, y, z - 1, EntityID.ID._Empty) == cLevelData.id)            bitmask += s;
            //else if (Level._instance.GetIfInRangeElseReturnInstead(x, y - 1, z - 1, EntityID.ID._Empty) == cLevelData.id)   bitmask += sb;
            if (Level._instance.GetIfInRangeElseReturnInstead(x - 1, y, z, EntityID.ID._Empty) == cLevelData.id)            bitmask += w;
            //else if (Level._instance.GetIfInRangeElseReturnInstead(x - 1, y - 1, z, EntityID.ID._Empty) == cLevelData.id)   bitmask += wb;
            if (Level._instance.GetIfInRangeElseReturnInstead(x + 1, y, z, EntityID.ID._Empty) == cLevelData.id)            bitmask += e;
            //else if (Level._instance.GetIfInRangeElseReturnInstead(x + 1, y - 1, z, EntityID.ID._Empty) == cLevelData.id)   bitmask += eb;
            if (Level._instance.GetIfInRangeElseReturnInstead(x, y, z + 1, EntityID.ID._Empty) == cLevelData.id)            bitmask += n;
            //else if (Level._instance.GetIfInRangeElseReturnInstead(x, y - 1, z + 1, EntityID.ID._Empty) == cLevelData.id)   bitmask += nb;

            SetMeshFromBitmask(entity, cMesh, cRotation, bitmask);
        });
    }

    private void SetMeshFromBitmask(Entity entity, C_HasMesh cMesh, Rotation cRotation, int bitmask)
    {
        switch (bitmask)
        {
            // no top or bottoms
            case 0:                         cMesh.mesh = Resource.Mesh.Block_RoundLone;         cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case s:                         cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case w:                         cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case s + w:                     cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case e:                         cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case s + e:                     cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case w + e:                     cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case s + w + e:                 cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case n:                         cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case n + s:                     cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case n + w:                     cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case n + s + w:                 cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case n + e:                     cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case n + e + s:                 cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case n + e + w:                 cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case n + e + w + s:             cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0);    break;

            // top but no bottoms
            case t:                         cMesh.mesh = Resource.Mesh.Block_RoundLone;         cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case t + s:                     cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case t + w:                     cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case t + s + w:                 cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case t + e:                     cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case t + s + e:                 cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case t + w + e:                 cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case t + s + w + e:             cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0);  break;
            case t + n:                     cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case t + n + s:                 cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case t + n + w:                 cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case t + n + s + w:             cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0);    break;
            case t + n + e:                 cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case t + n + e + s:             cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0);  break;
            case t + n + e + w:             cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0);   break;
            case t + n + e + w + s:         cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0);    break;








            // 1 bottom

            // south bottom only, no top
            case sb:                        cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case sb + w:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case sb + e:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case sb + w + e:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + sb:                    cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case n + sb + w:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case n + e + sb:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case n + e + w + sb:            cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south bottom only, with top
            case t + sb:                    cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + sb + w:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + sb + e:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + sb + w + e:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + sb:                cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + n + sb + w:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + n + e + sb:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + n + e + w + sb:        cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west bottom only, no top
            case wb:                        cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case s + wb:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case wb + e:                    cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case s + wb + e:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + wb:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case n + s + wb:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case n + e + wb:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case n + e + wb + s:            cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west bottom only, with top
            case t + wb:                    cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + s + wb:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + wb + e:                cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + s + wb + e:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + wb:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + n + s + wb:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + n + e + wb:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + n + e + wb + s:        cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // east bottom only, no top
            case eb:                        cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case s + eb:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case w + eb:                    cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case s + w + eb:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + eb:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case n + eb + s:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case n + eb + w:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case n + eb + w + s:            cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // east bottom only, with top
            case t + eb:                    cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + s + eb:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + w + eb:                cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + s + w + eb:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + eb:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + n + eb + s:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + n + eb + w:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + n + eb + w + s:        cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // north bottom only, no top
            case nb:                        cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + s:                    cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + w:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + s + w:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + e:                    cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + e + s:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + e + w:                cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + e + w + s:            cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // north bottom only, with top
            case t + nb:                    cMesh.mesh = Resource.Mesh.Block_RoundEnd;          cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + s:                cMesh.mesh = Resource.Mesh.Block_Lane;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + w:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + s + w:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + e:                cMesh.mesh = Resource.Mesh.Block_RoundCorner;       cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + e + s:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + e + w:            cMesh.mesh = Resource.Mesh.Block_Edge;              cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + e + w + s:        cMesh.mesh = Resource.Mesh.Block_Mid;               cRotation.Value = Quaternion.Euler(0, 0, 0); break;










            // 2 bottoms

            // south and west bottoms, no top
            case sb + wb:                   cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case sb + wb + e:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + sb + wb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case n + e + wb + sb:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south and west bottoms, with top
            case t + sb + wb:               cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + sb + wb + e:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + sb + wb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + n + e + wb + sb:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south and east bottoms, no top
            case sb + eb:                   cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case sb + w + eb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + eb + sb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case n + eb + w + sb:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south and east bottoms, with top
            case t + sb + eb:               cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + sb + w + eb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + eb + sb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + n + eb + w + sb:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south and north bottoms, no top
            case nb + sb:                   cMesh.mesh = Resource.Mesh.Block_Lane; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + sb + w:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + e + sb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + e + w + sb:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south and north bottoms, with top
            case t + nb + sb:               cMesh.mesh = Resource.Mesh.Block_Lane; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + sb + w:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + e + sb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + e + w + sb:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west and east bottoms, no top
            case wb + eb:                   cMesh.mesh = Resource.Mesh.Block_Lane; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case s + wb + eb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + eb + wb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case n + eb + wb + s:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west and east bottoms, with top
            case t + wb + eb:               cMesh.mesh = Resource.Mesh.Block_Lane; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + s + wb + eb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + eb + wb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + n + eb + wb + s:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west and north bottoms, no top
            case nb + wb:                   cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + s + wb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + e + wb:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + e + wb + s:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west and north bottoms, with top
            case t + nb + wb:               cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + s + wb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + e + wb:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + e + wb + s:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // east and north bottoms, no top
            case nb + eb:                   cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + eb + s:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + eb + w:               cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + eb + w + s:           cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // east and north bottoms, with top
            case t + nb + eb:               cMesh.mesh = Resource.Mesh.Block_RoundCorner; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + eb + s:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + eb + w:           cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + eb + w + s:       cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;








            // 3 bottoms

            // south, west, and east bottoms, no top
            case sb + wb + eb:              cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case n + eb + wb + sb:          cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south, west, and east bottoms, with top
            case t + sb + wb + eb:          cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, -90, 0); break;
            case t + n + eb + wb + sb:      cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south, west, and north bottoms, no top
            case nb + sb + wb:              cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case nb + e + wb + sb:          cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south, west and north bottoms, with top
            case t + nb + sb + wb:          cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + e + wb + sb:      cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south, east, and north bottoms, no top
            case nb + eb + sb:              cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case nb + eb + w + sb:          cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // south, east, and north bottoms, with top
            case t + nb + eb + sb:          cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 180, 0); break;
            case t + nb + eb + w + sb:      cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west, east, and north bottoms, no top
            case nb + eb + wb:              cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case nb + eb + wb + s:          cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;

            // west, east, and north bottoms, with top
            case t + nb + eb + wb:          cMesh.mesh = Resource.Mesh.Block_Edge; cRotation.Value = Quaternion.Euler(0, 90, 0); break;
            case t + nb + eb + wb + s:      cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;






            // 4 bottoms
            case nb + eb + wb + sb:         cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;
            case t + nb + eb + wb + sb:     cMesh.mesh = Resource.Mesh.Block_Mid; cRotation.Value = Quaternion.Euler(0, 0, 0); break;


          



            default: return;
        }
        PostUpdateCommands.SetComponent(entity, cMesh);
        PostUpdateCommands.SetComponent(entity, cRotation);
    }
}