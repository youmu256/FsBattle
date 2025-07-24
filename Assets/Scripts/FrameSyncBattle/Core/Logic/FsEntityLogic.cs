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
    
    
    public class FsEntityLogic
    {
        public static int IdGenerate { get; private set; }
        public object InitData { get; private set; }
        
        public bool HasStarted { get; private set; }
        public int Id { get; private set; }
        public string TypeId { get; private set; }
        public Vector3 Position { get; private set; }
        //euler存在万向锁问题 考虑换成四元数?
        public Vector3 Euler { get; private set; }
        public int Team { get; protected set; }
        
        public PlayAnimParam AnimationReq { get; protected set; }
        private FsEntityInitData Data => InitData as FsEntityInitData;

        public FsEntityLogic()
        {
            this.Id = ++IdGenerate;
        }

        /// <summary>
        /// 创建时初始化
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="team"></param>
        /// <param name="entityTypeId"></param>
        /// <param name="initData"></param>
        public virtual void Init(FsBattleLogic battle,int team, string entityTypeId, object initData)
        {
            this.Team = team;
            this.InitData = initData;
            this.TypeId = entityTypeId;
            this.Position = Data.Position;
            this.Euler = Data.Euler;
        }

        public virtual void OnCreate(FsBattleLogic battle)
        {
            
        }

        public virtual void OnRemove(FsBattleLogic battle)
        {
            
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
        
        public FsEntityLogic PlayAnimation(PlayAnimParam animParam)
        {
            AnimationReq = animParam;
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
        
    }
}