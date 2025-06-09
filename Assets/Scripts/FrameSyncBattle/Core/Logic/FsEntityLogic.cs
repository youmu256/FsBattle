using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityLogic : IAnimationPlayable
    {
        public static int IdGenerate { get; private set; }
        public object InitData { get; private set; }
        public int Id { get; private set; }
        public string TypeId { get; protected set; }
        public Vector3 Position { get; protected set; }
        public Vector3 Euler { get; protected set; }

        public int Team { get; protected set; }

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
        }

        public virtual void OnCreate(FsBattleLogic battle)
        {
            
        }

        public virtual void OnRemove(FsBattleLogic battle)
        {
            this.View = null;
        }
        
        public virtual void LogicFrame(FsBattleLogic battle, FsCmd cmd)
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
            View?.Play(animParam);
        }

        #endregion
    }
}