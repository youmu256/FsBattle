using System;
using UnityEngine;

namespace FrameSyncBattle
{
    /// <summary>
    /// 弹道对象 实现跟踪目标打击
    /// </summary>
    public class FsMissileLogic : FsEntityLogic
    {

        protected override void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            base.LogicUpdate(battle, cmd);
            this.MissileFrame(battle, battle.FrameLength);
        }


        public string MissileModel;

        public FsUnitLogic Source;//来源 可能为空
        
        public FsUnitLogic Target;//目标 可能为空

        public Vector3 StartPosition;//起点

        public Vector3 TargetPosition;//目标位置

        public bool LockTarget;//是否锁定
        
        public float LockAngle;//超过锁定角度会导致锁定无效

        public float LockAngularSpeed;//转向速度

        public float UnLockKeepTime;//锁定失败后剩余时间
        
        public float Speed;//飞行速度

        public float Arc;//高度比率决定曲线
        
        public float SideSpin;//侧旋角度
        
        //runtime var
        public float ArcHeight { get; private set; }
        public Vector3 MissileBasePosition { get; private set; }
        public Vector3 MissileResultPosition { get; private set; }
        public Vector3 MissileDirection { get; private set; }
        public float ReachTime { get; private set; }
        public float CurveReachTime { get; private set; }
        public float Timer { get; private set; }
        public bool Finished { get; private set; }
        public Action<FsBattleLogic,FsMissileLogic,bool> FlyEndCallBack { get; private set; }
        public object BindData { get; private set; }
        
        public FsMissileLogic SetBase(string model,float speed,float arc, float sideSpin)
        {
            this.MissileModel = model;
            this.Speed = speed;
            this.Arc = arc;
            this.SideSpin = sideSpin;
            return this;
        }
        
        public FsMissileLogic Fire(FsUnitLogic source,object bindData,Action<FsBattleLogic,FsMissileLogic,bool> callback)
        {
            Source = source;
            return Fire(bindData,callback);
        }

        public FsMissileLogic Fire(object bindData,Action<FsBattleLogic,FsMissileLogic,bool> callback)
        {
            BindData = bindData;
            Timer = 0;
            Finished = false;
            MissileBasePosition = StartPosition;
            MissileResultPosition = MissileBasePosition;
            FlyEndCallBack = callback;

            this.SetPosition(MissileBasePosition).SetEuler(Quaternion.LookRotation(MissileDirection).eulerAngles);
            return this;
        }
        
        /// <summary>
        /// 设置锁定相关数据
        /// 注意锁定相关的判定 都是无曲线偏移的情况下计算的
        /// </summary>
        /// <param name="lockAngle">锁定失效角度</param>
        /// <param name="lockAngleSpeed">锁定调整角速度每秒</param>
        /// <param name="keepTime">锁定失效后投射物持续时间</param>
        /// <returns></returns>
        public FsMissileLogic SetLockAngle(float lockAngle,float lockAngleSpeed,float keepTime)
        {
            this.LockAngle = lockAngle;
            this.LockAngularSpeed = lockAngleSpeed;
            this.UnLockKeepTime = keepTime;
            return this;
        }
        public FsMissileLogic AimTarget(Vector3 start,FsUnitLogic target,bool lockTarget)
        {
            AimTarget(start,target.GetBeHitPosition());
            Target = target;
            LockTarget = lockTarget;
            return this;
        }
        public FsMissileLogic AimTarget(Vector3 start,Vector3 target)
        {
            StartPosition = start;
            TargetPosition = target;
            float distance = DistanceUtils.DistanceBetween(StartPosition, TargetPosition);
            ReachTime = CalReachTime(distance);
            CurveReachTime = ReachTime;
            MissileDirection = (TargetPosition - StartPosition).normalized;
            ArcHeight = distance * Arc;
            return this;
        }
        
        private float CalReachTime(float distance)
        {
            if (Speed <= 0)
            {
                return 0;
            }
            else
            {
                return distance / Speed;
            }
        }
        
        public void FlyEnd(FsBattleLogic battle,bool valid)
        {
            battle.RemoveEntity(this);
            if (FlyEndCallBack != null)
                FlyEndCallBack.Invoke(battle,this,valid);
            Finished = true;
        }

        private bool IsLockValid(FsUnitLogic target)
        {
            bool targetValid = IsTargetValid(target);
            if (LockAngle > 0)
            {
                //无曲线Offset的情况下的角度
                var dir = target.GetBeHitPosition() - MissileBasePosition;
                if (Vector3.Angle(MissileDirection, dir) > LockAngle)
                {
                    return false;
                }
            }
            return targetValid;
        }
        
        private bool IsTargetValid(FsUnitLogic target)
        {
            return target.IsRemoved == false;
        }
        
        
        private void MissileFrame(FsBattleLogic battle,float deltaTime)
        {
            if (Finished) return;
            //为了保证精准命中位置 要对step做限制
            var remainTime = ReachTime - Timer;
            if (deltaTime > ReachTime)
                deltaTime = remainTime;
            Timer += deltaTime;
            if (LockTarget)
            {
                TargetPosition = Target.GetBeHitPosition();
                if (IsLockValid(Target) == false)
                {
                    LockTarget = false;//锁定失效
                    ReachTime = Timer + UnLockKeepTime;//倒计时结束
                }
                else
                {
                    var aimDir = (TargetPosition - MissileBasePosition).normalized;
                    if (LockAngularSpeed > 0)
                    {
                        aimDir = Vector3.RotateTowards(MissileDirection, aimDir, LockAngularSpeed * deltaTime, 0);
                    }
                    MissileDirection = aimDir;
                    //update reach time
                    float distance = DistanceUtils.DistanceBetween(StartPosition, TargetPosition);
                    CurveReachTime = CalReachTime(distance);
                    ReachTime = Timer + 1f;//LockTarget模式下 不会因为时间结束
                }
            }
            //Vector3 modelPosition;
            bool targetPointHit = false;
            Vector3 curveOffset = Vector3.zero;
            Vector3 baseOffset;
            if (ReachTime > 0)
            {
                var baseStep = Speed * deltaTime;
                float distance = DistanceUtils.DistanceBetween(MissileBasePosition, TargetPosition);
                if (baseStep >= distance)
                {
                    baseStep = distance;
                    targetPointHit = true;
                }
                baseOffset = MissileDirection * baseStep;
                
                //curve offset calculate
                bool stopArc = Timer >= CurveReachTime;
                if (stopArc == false && ArcHeight > 0)//--Arc Process
                {
                    float px = Mathf.Min(Timer / CurveReachTime,1f);
                    float height = ArcHeight * Mathf.Sin(px * 180f * Mathf.Deg2Rad);
                    curveOffset = Quaternion.LookRotation(MissileDirection,Vector3.up)*(Quaternion.Euler(0, 0, SideSpin) * (Vector3.up * height));
                }
            }
            else
            {
                baseOffset = TargetPosition - MissileBasePosition;
            }
            
            MissileBasePosition += baseOffset;//基础路径推进
            Vector3 nextGoal = MissileBasePosition + curveOffset;
            Vector3 nextStep = nextGoal - MissileResultPosition;
            MissileResultPosition = MissileResultPosition + nextStep;
            this.SetPosition(MissileResultPosition).SetEuler(Quaternion.LookRotation(nextStep).eulerAngles);
            
            //时间到达 或者 移动到位
            if (ReachTime <= 0 || Timer >= ReachTime || targetPointHit)
            {
                FlyEnd(battle,true);
                return;
            }
            //目标体积内命中判定
            if (Target != null && Target.BeHitCheck(MissileResultPosition, 0))
            {
                FlyEnd(battle,true);
                return;
            }
        }
    }
}