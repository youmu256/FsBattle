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
        
        public PlayAnimParam Animation { get; protected set; }

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
            Animation = animParam;
        }

        #endregion
    }
}