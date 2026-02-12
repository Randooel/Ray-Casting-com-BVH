using System.Collections.Generic;
using UnityEngine;

// !!! This class can't inherit from Monobehavior !!!
public class BVHNodes
{
    public Bounds aabb;
    public BVHNodes left;
    public BVHNodes right;
    public List<Collider> leafCollider = new List<Collider>();

    public bool IsLeaf => left == null && right == null;

    public bool Intersects(Ray ray, out float distance)
    {
        return aabb.IntersectRay(ray, out distance);
    }
}
