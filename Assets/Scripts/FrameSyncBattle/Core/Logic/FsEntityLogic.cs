using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityLogicViewSnapshot{
        //有2个快照就能让渲染层进行插值显示了
        public Vector3 Position;
        public Vector3 Euler;
    }
    
    public class FsEntityLogic : IAnimationPlayable
    {
        public static int IdGenerate { get; private set; }
        public object InitData { get; private set; }
        
        public bool HasStarted { get; private set; }
        public int Id { get; private set; }
        public string TypeId { get; protected set; }
        public Vector3 Position { get; protected set; }
        public Vector3 Euler { get; protected set; }
        public int Team { get; protected set; }
        //public string Animation { get; protected set; }

        public FsEntityLogicViewSnapshot CreateViewSnapshot()
        {
            FsEntityLogicViewSnapshot shot = new FsEntityLogicViewSnapshot();
            shot.Position = this.Position;
            shot.Euler = this.Euler;
            return shot;
        }

        private FsEntityInitData Data => InitData as FsEntityInitData;

        public FsEntityLogic()
        {
            this.Id = ++IdGenerate;
        }

        /// <summary>
        /// 创建时初始化
        /// </summary>
        /// <param name="team"></param>
        /// <param name="entityTypeId"></param>
        /// <param name="initData"></param>
        public virtual void Init(int team, string entityTypeId, object initData)
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
            this.View = null;
        }
        
        /// <summary>
        /// 逻辑帧驱动 对象创建出来后也会立刻执行一次
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="cmd"></param>
        public void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
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
        
        #region View抽象绑定
        
        public IFsEntityView View { get; private set; }

        public void BindView(IFsEntityView view)
        {
            this.View = view;
        }

        public void Play(PlayAnimParam animParam)
        {
            /*
             * TODO
             * 目前存在一个问题
             * 让表现层对象去播放动画目前存在的问题，因为View层要进行插值，所以实际上是晚一个逻辑帧的
             * 比如逻辑帧里移动到指定为止后开始播放攻击动画，在表现层实际上会还没到位置就开始播放攻击动画
             * 考虑是否表现层应该也严格遵守插值渲染的流程
             */
            View?.Play(animParam);
        }

        #endregion
    }
}