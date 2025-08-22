
using System.Collections.Generic;

namespace FrameSyncBattle
{
    /// <summary>
    /// 除了基础逻辑外 还有对接一些渲染和指令输入
    /// </summary>
    public class FsBattleGame : FsBattleLogic
    {
        #region 渲染相关

        /*
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
            if (entity.View is FsEntityView view)
            {
                entity.BindView(null);
                view.OnRemove(this);
                EntityViews.Remove(view);
            }
            base.RemoveEntity(entity);
        }
        */
        #endregion
        
        public float ViewLerp { get; private set; }
        
        public int ViewLerpStartFrame { get; private set; }
        
        public FsLinkedList<FsEntityView> EntityViews = new();

        public Dictionary<int, FsEntityView> EntityViewsMap = new();
        
        public void GameEngineUpdate(float deltaTime,FsCmd cmd)
        {
            //准备渲染插值进度增长
            ViewLerp += (deltaTime * 1f / this.FrameLength);
            if (ViewLerp > 1f)
                ViewLerp = 1f;
            if (IsReplayMode)
            {
                this.ReplayUpdate(deltaTime);
            }
            else
            {
                this.Update(deltaTime,cmd);
            }
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

            //渲染对象的增删先处理 等于是延迟一个逻辑帧做处理 这样能对的上表现层的位置插值晚一帧
            SyncViewToLogic();
            //渲染对象准备插值 得在逻辑对象更新之前 主要是记录上一逻辑状态
            var game = (this);
            EntityViews.RefForEach(ref game,((view, param) =>
            {
                view.BeforeLogicFrame(param,param.ViewLerp);
            }));
            //所有逻辑对象更新
            base.GameLogicFrame(cmd);
        }

        private void SyncViewToLogic()
        {
            //移除无对应逻辑对象的渲染对象
            EntityViews.ForEach(view =>
            {
                var valid = this.EntityService.EntitiesMap.ContainsKey(view.Id);
                if (valid == false)
                {
                    view.OnRemove(this);
                    EntityViews.Remove(view);
                    EntityViewsMap.Remove(view.Id);
                }
            });

            //新增渲染对象
            this.Entities.ForEach(logic =>
            {
                var valid = this.EntityViewsMap.ContainsKey(logic.Id);
                if (valid == false)
                {
                    var view = FsEntityView.Create(logic);
                    view.OnCreate(this);
                    EntityViews.Add(view);
                    EntityViewsMap.Add(view.Id, view);
                }
            });
        }

        public override void EndBattle()
        {
            base.EndBattle();
            SyncViewToLogic();
        }
    }
}