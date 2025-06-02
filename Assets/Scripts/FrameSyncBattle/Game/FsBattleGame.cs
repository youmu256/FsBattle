using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    /// <summary>
    /// 除了基础逻辑外 还有对接一些渲染和指令输入
    /// </summary>
    public class FsBattleGame : FsBattleLogic
    {
        public FsBattleGame(int fps) : base(fps)
        {
        }

        public void StartBattle(BattleStartData startData)
        {
            //创建玩家
            this.CreateEntity<FsPlayerLogic>("player",
                new FsUnitInitData() { Euler = Vector3.zero, Position = Vector3.zero });
        }

        #region 渲染相关

        public override T CreateEntity<T>(string entityTypeId, object initData)
        {
            var logic = base.CreateEntity<T>(entityTypeId, initData);
            var view = FsEntityView.Create(logic);
            EntityViews.Add(view);
            return logic;
        }

        #endregion
        
        public float ViewLerp { get; private set; }
        
        public List<FsEntityView> EntityViews = new();

        public void GameEngineUpdate(float deltaTime,FsCmd cmd)
        {
            //准备渲染插值
            int change = this.Update(deltaTime,cmd);
            //计算表现插值 上一逻辑帧的位置插值到最新逻辑帧
            ViewLerp += (deltaTime * 1f / this.FrameLength);
            if (ViewLerp > 1f)
                ViewLerp = 1f;
            foreach (var view in EntityViews)
            {
                view.ViewInterpolation(this,ViewLerp);
            }
        }

        protected override void GameLogicFrame(FsCmd cmd)
        {
            //立刻完成之前的插值表现
            foreach (var view in EntityViews)
            {
                view.PrepareLerp(this);
            }
            ViewLerp = 0;
            base.GameLogicFrame(cmd);
        }
    }
}