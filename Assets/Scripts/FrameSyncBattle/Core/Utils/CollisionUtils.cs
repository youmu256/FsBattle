
using System.Collections.Generic;
using UnityEngine;
namespace FrameSyncBattle
{
    public static class CollisionUtil 
    {
        public static bool RaySphereIntersect(Vector3 start, Vector3 dir, Vector3 center, float radius, out Vector3 point)
        {
            return RaySphereIntersect(new Ray(start, dir),dir.magnitude, center, radius, out point);
        }
        public static bool RaySphereIntersect(Ray ray ,float dis, Vector3 center, float radius, out Vector3 point)
        {
            point = Vector3.zero;
            Vector3 oc = ray.origin - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
        
            float delta = b * b - 4 * a * c;
            if (delta < 0) return false;

            float sqrtDelta = Mathf.Sqrt(delta);
            float t1 = (-b - sqrtDelta) / (2 * a);
            if (t1 >= 0 && t1 < dis)
            {
                point = ray.GetPoint(t1);
                return true;
            }
            float t2 = (-b + sqrtDelta) / (2 * a);
            if (t2 >= 0 && t2 < dis)
            {
                point = ray.GetPoint(t2);
                return true;
            }
            return false;
        }
    }
}