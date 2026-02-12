using System.Collections.Generic;
using UnityEngine;

public class EnemiesBVH : MonoBehaviour
{
    public List<Collider> upperBodyParts;
    public List<Collider> lowerBodyParts;

    public BVHNodes rootNode;

    void Start()
    {
        BuildHierarchy();
    }

    void BuildHierarchy()
    {
        rootNode = new BVHNodes();

        BVHNodes upperBodyNode = BuildBranch(upperBodyParts);
        BVHNodes lowerBodyNode = BuildBranch(lowerBodyParts);
        rootNode.left = upperBodyNode;
        rootNode.right = lowerBodyNode;

        rootNode.aabb = EncapsulateBounds(upperBodyNode.aabb, lowerBodyNode.aabb);
    }

    BVHNodes BuildBranch(List<Collider> parts)
    {
        BVHNodes node = new BVHNodes();

        node.leafCollider = new List<Collider>(parts);

        Bounds combinedBounds = new Bounds();
        if (parts.Count > 0)
        {
            combinedBounds = parts[0].bounds;
            for (int i = 1; i < parts.Count; i++)
            {
                combinedBounds.Encapsulate(parts[i].bounds);
            }
        }

        node.aabb = combinedBounds;
        return node;
    }


    void Update()
    {
        RefitBVH();
    }

    void RefitBVH()
    {
        if (rootNode == null) return;


        UpdateBranchBounds(rootNode.left, upperBodyParts);

        UpdateBranchBounds(rootNode.right, lowerBodyParts);


        rootNode.aabb = EncapsulateBounds(rootNode.left.aabb, rootNode.right.aabb);
    }

    void UpdateBranchBounds(BVHNodes node, List<Collider> parts)
    {
        if (parts.Count == 0) return;

        Bounds b = parts[0].bounds;
        foreach (var p in parts) b.Encapsulate(p.bounds);
        node.aabb = b;
    }

    Bounds EncapsulateBounds(Bounds b1, Bounds b2)
    {
        Bounds b = b1;
        b.Encapsulate(b2);
        return b;
    }

    public bool CheckHit(Ray ray, out RaycastHit finalHit)
    {
        finalHit = new RaycastHit();

        if (!rootNode.Intersects(ray, out float dist)) return false;

        bool hitUpper = rootNode.left.Intersects(ray, out float distUp);
        bool hitLower = rootNode.right.Intersects(ray, out float distLow);

        if (hitUpper)
        {
            if (CheckDetailedCollision(ray, upperBodyParts, out finalHit)) return true;
        }

        if (hitLower)
        {
            if (CheckDetailedCollision(ray, lowerBodyParts, out finalHit)) return true;
        }
        return false;
    }

    bool CheckDetailedCollision(Ray ray, List<Collider> parts, out RaycastHit hitInfo)
    {
        float minDst = float.MaxValue;
        bool hasHit = false;
        hitInfo = new RaycastHit();

        foreach (var col in parts)
        {
            if (col.Raycast(ray, out RaycastHit tempHit, 1000f))
            {
                if (tempHit.distance < minDst)
                {
                    minDst = tempHit.distance;
                    hitInfo = tempHit;
                    hasHit = true;
                }
            }
        }
        return hasHit;
    }


    void OnDrawGizmos()
    {
        if (rootNode == null) return;


        Gizmos.color = Color.orange;
        Gizmos.DrawWireCube(rootNode.aabb.center, rootNode.aabb.size);


        if (rootNode.left != null)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawWireCube(rootNode.left.aabb.center, rootNode.left.aabb.size);
        }

        if (rootNode.right != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rootNode.right.aabb.center, rootNode.right.aabb.size);
        }
    }
}
