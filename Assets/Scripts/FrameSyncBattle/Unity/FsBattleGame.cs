using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace FrameSyncBattle
{
    /// <summary>
    /// 除了基础逻辑外 还有对接一些渲染和指令输入
    /// </summary>
    public class FsBattleGame : FsBattleLogic
    {
        public FsBattleGame(int fps, int seed) : base(fps, seed)
        {
        }

        public void StartBattle(BattleStartData startData)
        {
            //创建玩家
            this.AddEntity<FsPlayerLogic>(FsBattleLogic.PlayerTeam, "player",
                new FsUnitInitData() {Euler = Vector3.zero, Position = Vector3.zero});
            //根据data创建敌人
            this.AddEntity<FsUnitLogic>(FsBattleLogic.EnemyTeam, "enemy",
                new FsUnitInitData() {Euler = Vector3.zero, Position = Vector3.forward});
        }

        #region 渲染相关

        public override T AddEntity<T>(int team,string entityTypeId, object initData)
        {
            var logic = base.AddEntity<T>(team,entityTypeId, initData);
            var view = FsEntityView.Create(logic);
            logic.BindView(view);
            EntityViews.Add(view);
            return logic;
        }

        public override void RemoveEntity(FsEntityLogic entity)
        {
            base.RemoveEntity(entity);
            if (entity.View is FsEntityView view)
            {
                entity.BindView(null);
                view.OnRemove(this);
                EntityViews.Remove(view);
            }
        }

        #endregion
        
        public float ViewLerp { get; private set; }
        
        public int ViewLerpStartFrame { get; private set; }
        
        public FsLinkedList<FsEntityView> EntityViews = new();

        public void GameEngineUpdate(float deltaTime,FsCmd cmd)
        {
            //准备渲染插值进度增长
            ViewLerp += (deltaTime * 1f / this.FrameLength);
            if (ViewLerp > 1f)
                ViewLerp = 1f;
            
            int change = this.Update(deltaTime,cmd);
            
            //最后再应用表现插值
            foreach (var view in EntityViews)
            {
                view.ViewInterpolation(ViewLerp);
            }
            //Debug.Log($"logic change {change}");
        }

        protected override void GameLogicFrame(FsCmd cmd)
        {
            //逻辑帧更新之前重置插值进度 总是让上一逻辑帧状态往最新逻辑帧状态插值
            ViewLerpStartFrame = FrameIndex;
            //ViewLerp = 0;
            //貌似会出现一些匀速移动上的不连贯 也可能是眼花
            //让渲染进度直接保留逻辑模拟时间增量来尽量维持顺滑
            ViewLerp = Accumulator / FrameLength;

            var game = (this);
            EntityViews.ForEach(ref game,((view, param) =>
            {
                view.PrepareLerp(param,param.ViewLerp);
            }));
            /*
            foreach (var view in EntityViews)
            {
                view.PrepareLerp(this,ViewLerp);
            }
            */
            //所有逻辑对象更新
            base.GameLogicFrame(cmd);
        }
    }
}