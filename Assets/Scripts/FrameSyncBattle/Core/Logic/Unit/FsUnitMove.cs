using UnityEngine;

namespace FrameSyncBattle
{
    
    
    public enum MoveStepType
    {
        UnitMove,
        ExtraMove,
        Gravity,
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

        /// <summary>
        /// 增量移动
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="moveType"></param>
        void MoveStep(Vector3 offset, MoveStepType moveType);

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
        void SetMoveSpeed(float speed);
        float GetMoveSpeed();
    }

    public interface IMoveService : IMoveServiceCaller,IFsEntityFrame
    {
    }
    
    public class FsSimpleMoveService : IMoveService
    {
        public FsUnitLogic Owner { get; private set; }
        public float Speed { get; private set; }
        public float StopDistance { get; private set; }
        public bool MoveIsPause { get; set; }
        public void SetMoveSpeed(float speed)
        {
            Speed = speed;
        }

        public float GetMoveSpeed()
        {
            return Speed;
        }

        public FsSimpleMoveService(FsUnitLogic owner)
        {
            this.Owner = owner;
            this.Speed = 0f;
        }

        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            //move to target
            if (MoveIsPause) return;
            if (IsMoveFinished()) return;
            //实现移动
        }
        
        public bool IsMoveFinished()
        {
            return false;
        }

        public bool MoveToPosition(Vector3 target, float stopDistance)
        {
            Vector3 start = Owner.Position;
            bool success = false;
            
            //需要检查移动命令是否有效
            
            if (success)
            {
                this.StopDistance = stopDistance;
            }
            return success;
        }

        public void StopMove()
        {
            
        }

        public void MoveStep(Vector3 offset, MoveStepType moveType)
        {
            
        }
    }

    public class FsUnitMove
    {
        
    }
}