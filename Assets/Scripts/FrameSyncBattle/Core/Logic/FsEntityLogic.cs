using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityLogicViewSnapshot{
        //有2个快照就能让渲染层进行插值显示了
        public Vector3 Position;
        public Vector3 Euler;
    }
    
    //entity的自身表现控制需求有 播放动画
    //如果要对特效/音效进行控制 那特效对象应该也作为一个逻辑entity

    public interface IFsEntityLogic
    {
        void Start();
        void Update();
        void Remove();
    }

    /// <summary>
    /// 基础的逻辑对象
    /// 拥有模型和动画播放
    /// </summary>
    public class FsEntityLogic
    {
        public object InitData { get; private set; }
        public bool HasStarted { get; private set; }
        public int Id { get; private set; }
        /**EntityType 记录Entity的类别 用于区分*/
        public FsEntityType EntityType { get; private set; }
        public Vector3 Position { get; private set; }
        //euler存在万向锁问题 考虑换成四元数?
        public Vector3 Euler { get; private set; }
        public int Team { get; protected set; }
        public string ViewModel { get; set; }
        public float ViewModelScale { get; set; }
        public float RemainLiveTime { get; private set; }
        
        public FsEntityLogic SetModel(string model, float scale)
        {
            ViewModel = model;
            ViewModelScale = scale;
            return this;
        }
        
        public PlayAnimParam AnimationReq { get; protected set; }
        private FsEntityInitData Data => InitData as FsEntityInitData;
        public FsEntityLogic()
        {
        }

        /// <summary>
        /// 创建时初始化
        /// initData好像有点多余啊 每个具体子类型自己去额外设置会比较好?
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="team"></param>
        /// <param name="entityType"></param>
        /// <param name="initData"></param>
        public virtual void Init(FsBattleLogic battle,int team, FsEntityType entityType, object initData)
        {
            this.Id = battle.EntityIdGenerator;
            this.Team = team;
            this.InitData = initData;
            this.EntityType = entityType;
        }

        public virtual void OnCreate(FsBattleLogic battle)
        {
            
        }

        public virtual void OnRemove(FsBattleLogic battle)
        {
            
        }

        private const float OneFrameLiveTime = 0.01f;
        /// <summary>
        /// 设置剩余存活时间
        /// </summary>
        /// <param name="time">不填则下一帧就会移除</param>
        public FsEntityLogic SetLiveTime(float time = OneFrameLiveTime)
        {
            RemainLiveTime = time;
            return this;
        }

        protected virtual void OnLiveTimeEnd(FsBattleLogic battle)
        {
            RemainLiveTime = 0;
            battle.RemoveEntity(this);
        }
        
        /// <summary>
        /// 逻辑帧驱动 对象创建出来后也会立刻执行一次
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="cmd"></param>
        public void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            OnLogicFrameStart();
            if (HasStarted)
            {
                LogicUpdate(battle,cmd);
                if (RemainLiveTime > 0)
                {
                    RemainLiveTime-=battle.FrameLength;
                    if (RemainLiveTime <= 0)
                        OnLiveTimeEnd(battle);
                }
            }
            else
            {
                HasStarted = true;
                LogicStart(battle,cmd);
            }
        }

        protected virtual void LogicStart(FsBattleLogic battle, FsCmd cmd)
        {
            
        }

        protected virtual void LogicUpdate(FsBattleLogic battle, FsCmd cmd)
        {
            
        }

        protected void OnLogicFrameStart()
        {
            //因为Animation 在目前是在View层延迟处理的 更像是请求 这里相当于进行重置
            AnimationReq = PlayAnimParam.Null;
        }


        public FsEntityLogic PlayAnimation(string animation)
        {
            AnimationReq = new PlayAnimParam(animation);
            return this;
        }
        
        public FsEntityLogic PlayAnimation(PlayAnimParam animParam)
        {
            AnimationReq = animParam;
            return this;
        }

        public FsEntityLogic SetToAnglePoint(FsAnglePoint anglePoint)
        {
            if (anglePoint == null) return this;
            SetPosition(anglePoint.Position).SetEuler(anglePoint.Euler);
            return this;
        }

        public FsEntityLogic SetPosition(Vector3 position)
        {
            Position = position;
            return this;
        }

        public FsEntityLogic SetEuler(Vector3 euler)
        {
            Euler = euler;
            return this;
        }

        public virtual string DebugMsg()
        {
            return $"id:{this.Id},pos:{this.Position},euler:{this.Euler}";
        }
    }

}