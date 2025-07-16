using UnityEngine;

namespace FrameSyncBattle
{
    public class DistanceUtils
    {
        public static float DistanceBetween2D(Vector3 start, Vector3 target)
        {
            var d = target - start;
            d.y = 0;
            return d.magnitude;
        }
        
        public static float DistanceBetween2D(FsUnitLogic source, Vector3 target,bool calEntityRadius)
        {
            float distance = DistanceBetween2D(source.Position,target);
            if (calEntityRadius)
            {
                distance -= source.Radius;
            }
            return distance;
        }
        public static float DistanceBetween2D(FsUnitLogic source, FsUnitLogic target,bool calEntityRadius)
        {
            float distance = DistanceBetween2D(source.Position,target.Position);
            if (calEntityRadius)
            {
                distance -= source.Radius;
            }
            return distance;
        }

        public static float DistanceBetween(Vector3 start, Vector3 target)
        {
            return (target - start).magnitude;
        }
        
        public static float DistanceBetween(FsUnitLogic source, FsUnitLogic target,bool calEntityRadius)
        {
            float distance = DistanceBetween(source.Position, target.Position);
            if (calEntityRadius)
            {
                distance -= (source.Radius + target.Radius);
            }
            return distance;
        }
        public static float DistanceBetween(FsUnitLogic source, Vector3 target,bool calEntityRadius)
        {
            float distance = DistanceBetween(source.Position, target);
            if (calEntityRadius)
            {
                distance -= source.Radius;
            }
            return distance;
        }
        public static bool IsReachTarget2D(FsUnitLogic entity,FsUnitLogic target,float reachDistance)
        {
            if (DistanceBetween2D(entity, target, true) <= reachDistance)
            {
                return true;
            }
            return false;
        }
        public static bool IsReachPosition2D(FsUnitLogic entity,bool checkMyRadius,Vector3 position,float reachDistance)
        {
            if (DistanceBetween2D(entity, position, checkMyRadius) <= reachDistance)
            {
                return true;
            }
            return false;
        }
    }
}