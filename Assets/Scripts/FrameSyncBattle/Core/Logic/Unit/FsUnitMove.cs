using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    
    
    public enum MoveStepType
    {
        UnitMove,
        ExtraMove,
    }

    public interface IMoveServiceCaller
    {
        /// <summary>
        /// 移动到目标点
        /// </summary>
        /// <param name="target"></param>
        /// <param name="stopDistance"></param>
        /// <returns></returns>
        bool MoveToPosition(Vector3 target, float stopDistance);

        /// <summary>
        /// 停止移动
        /// </summary>
        void StopMove();

        void MoveStep(Vector3 step, MoveStepType moveType);

        /// <summary>
        /// 是否移动完毕了 没开始移动的时候待着也算移动完毕 或者叫移动模块处于停止
        /// </summary>
        /// <returns></returns>
        bool IsMoveFinished();
        /// <summary>
        /// 是否被暂停了
        /// </summary>
        /// <returns></returns>
        bool MoveIsPause { get; set; }
        void UpdateMoveSpeed(float speed);
    }

    public interface IMoveService : IMoveServiceCaller,IFsEntityFrame
    {
    }

    public enum PathMoveState
    {
        None,
        Moving,
        Finished,
    }
    
    public class FsSimpleMoveService : IMoveService
    {
        public FsUnitLogic Owner { get; private set; }
        public float Speed { get; private set; }
        public float StopDistance { get; private set; }
        
        public bool MoveIsPause { get; set; }
        public void UpdateMoveSpeed(float speed)
        {
            Speed = speed;
        }
        public FsSimpleMoveService(FsUnitLogic owner)
        {
            this.Owner = owner;
            this.Speed = 0f;
        }

        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            if (MoveIsPause == false && PathMoving && PathMoveFinished == false)
            {
                //实现移动
                var next = PathPoints[PathMoveNext];
                var toNext = next - Owner.Position;
                var dis = DistanceUtils.DistanceBetween2D(Owner, next, false);
                var moveStepDis = Speed * deltaTime;
                if (moveStepDis >= dis)
                {
                    moveStepDis = dis;
                    PathMoveNext++;
                    if (PathMoveNext >= PathPoints.Count)
                    {
                        PathMoveFinished = true;
                    }
                }

                var step = moveStepDis * toNext.normalized;
                this.MoveStep(step, MoveStepType.UnitMove);
            }
        }

        public bool IsMoveFinished()
        {
            if (PathMoving)
                return PathMoveFinished;
            return false;
        }

        protected int PathMoveNext { get; private set; }
        protected bool PathMoving { get; private set; }
        protected bool PathMoveFinished { get; private set; }
        protected List<Vector3> PathPoints { get; } = new();
        
        public bool MoveToPosition(Vector3 target, float stopDistance)
        {
            //默认都无障碍 也不需要寻路
            PathMoveFinished = false;
            PathMoveNext = 0;
            PathPoints.Clear();
            PathPoints.Add(target);
            PathMoving = true;

            StopDistance = stopDistance;
            return true;
        }

        public void StopMove()
        {
            StopDistance = 0;
            PathMoveFinished = false;
            PathMoveNext = 0;
            PathPoints.Clear();
            PathMoving = false;
        }

        public void MoveStep(Vector3 step, MoveStepType moveType)
        {
            step.y = 0;
            Owner.SetPosition(Owner.Position + step);
        }
    }

    public class FsUnitMove
    {
        
    }
}