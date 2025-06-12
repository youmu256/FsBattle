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