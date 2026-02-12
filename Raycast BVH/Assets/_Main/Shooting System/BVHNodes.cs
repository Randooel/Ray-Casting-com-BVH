using UnityEngine;

// !!! This class can't inherit from Monobehavior !!!
public class BVHNodes
{
    public Bounds aabb;
    public BVHNodes left;
    public BVHNodes right;
    public Collider leafCollider;

    public bool Intersects(Ray ray, out float distance)
    {
        return aabb.IntersectRay(ray, out distance);
    }
}
