using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace FrameSyncBattle
{

    public class CreateEntityEvent : GameEvent
    {
        public FsEntityLogic EntityLogic;
        public CreateEntityEvent(FsEntityLogic entityLogicLogic)
        {
            this.EntityLogic = entityLogicLogic;
        }
        
        public override void Reset()
        {
            
        }
    }

    public class FsEntityService
    {
        public readonly List<FsUnitLogic> Units = new();

        public Dictionary<int, FsEntityLogic> EntitiesMap = new();
        
        public void UpdateEntityCache(FsEntityLogic entity,bool isAdd)
        {
            if (isAdd)
            {
                EntitiesMap.Add(entity.Id,entity);
                if (entity is FsUnitLogic unit)
                {
                    Units.Add(unit);
                }
            }
            else
            {
                EntitiesMap.Remove(entity.Id);
                if (entity is FsUnitLogic unit)
                {
                    Units.Remove(unit);
                }
            }
        }
        protected readonly List<FsUnitLogic> TempList = new();

        public void CollectUnitsInPositionRange2D(ICollection<FsUnitLogic> container,Vector3 position,float range,Func<FsUnitLogic,bool> filter)
        {
            TempList.Clear();
            var aoiList = TempList;
            foreach (var unit in Units)
            {
                if (DistanceUtils.DistanceBetween2D(unit, position,true) <= range)
                {
                    aoiList.Add(unit);
                }
            }
            foreach (var unit in aoiList)
            {
                if (filter.Invoke(unit) == false) continue;
                container.Add(unit);
            }
            TempList.Clear();
        }
        
        
        /// <summary>
        /// 目标对象是否能被作为AI目标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsEntityValidTobeTargeted(FsUnitLogic source,FsUnitLogic target)
        {
            if (source == null) return false;
            if (target == null) return false;
            if (target.IsDead || target.IsRemoved) return false;
            return true;
        }
    }

    public partial class FsBattleLogic
    {
        /// <summary>
        /// 处理伤害
        /// </summary>
        /// <param name="info"></param>
        public void ProcessDamage(FsDamageInfo info)
        {
            if (info.Target == null || info.Target.IsDead) return;
            //角色身上的技能&Buff需要响应受伤事件
            
            //按照游戏公式等修正伤害
            info.Target.HpCurrent -= info.Damage;
            info.Target.OnDamagedPost(this,info);
        }
    }
    
    public partial class FsBattleLogic
    {
        public FsEntityService EntityService { get; private set; } = new();
        
        //public GameEventHandler EventHandler { get; private set; } = new GameEventHandler();
        public virtual T AddEntity<T>(int team,string entityTypeId, object initData) where T : FsEntityLogic, new()
        {
            var entity = new T();
            entity.Init(team,entityTypeId, initData);
            entity.OnCreate(this);
            Entities.Add(entity);
            EntityService.UpdateEntityCache(entity,true);
            return entity;
        }

        public virtual void RemoveEntity(FsEntityLogic entity)
        {
            entity.OnRemove(this);
            Entities.Remove(entity);
            EntityService.UpdateEntityCache(entity,false);
        }

    }

    public partial class FsBattleLogic
    {
        public const int PlayerTeam = 0;
        public const int EnemyTeam = 1;
        public bool IsReplayMode { get; private set; } = false;

        public Random RandomGen { get; private set; }
        public void Init(int fps,int seed,FsBattleStartData startData)
        {
            IsReplayMode = false;
            Fps = fps;
            RandomGen = new Random(seed);
            //replay save init
            ReplaySave = new FsBattleReplay();
            ReplaySave.Init(fps,seed,startData);
            InitBattleEntities(startData);
        }

        public void InitByReplay(FsBattleReplay replay)
        {
            IsReplayMode = true;
            this.Fps = replay.Fps;
            this.RandomGen = new Random(replay.Seed);
            ReplaySave = null;
            Replay = replay;
            InitBattleEntities(replay.StartData);
        }

        private void InitBattleEntities(FsBattleStartData startData)
        {
            //battle unitss init
            foreach (var unitData in startData.PlayerTeamUnits)
            {
                this.AddEntity<FsPlayerLogic>(PlayerTeam, unitData.TypeId, unitData.UnitInitData);
            }
            foreach (var unitData in startData.EnemyTeamUnits)
            {
                this.AddEntity<FsUnitLogic>(EnemyTeam, unitData.TypeId, unitData.UnitInitData);
            }
        }
        

        public void StartBattle()
        {
        }

        public virtual void CleanBattle()
        {
            var p = (this);
            Entities.RefForEach(ref p, ((logic, param) => { param.RemoveEntity(logic); }));
        }

        protected FsLinkedList<FsEntityLogic> Entities = new();
        
        #region 逻辑帧相关
        public float LogicTime { get; private set; }
        protected float Accumulator { get; private set; }
        public int FrameIndex { get; private set; }
        public int Fps { get; private set; }
        public float FrameLength
        {
            get
            {
                return 1f / Fps;
            }
        }

        #endregion
        
        #region 多操作合并
        protected List<FsCmd> SubmitCmdCache = new();
        /*从缓存操作中合并操作得到一个逻辑帧操作*/
        private FsCmd MergeCmdList(List<FsCmd> cmdList)
        {
            //因为逻辑帧远低于渲染帧率
            //为了正确响应操作手感 要对逻辑帧时缓存的所有操作进行处理 返回一个满足手感调优后的操作对象
            //否则比如按下按键的响应经常会丢
            var cmd = new FsCmd();
            //逻辑帧之前任意一帧按下的按键都会保留
            foreach (var fsCmd in cmdList)
            {
                cmd.Buttons |= fsCmd.Buttons;
                if(fsCmd.ButtonContains(FsButton.Fire))
                    cmd.FireYaw = fsCmd.FireYaw;
            }
            cmd.LogicFrameIndex = FrameIndex;
            return cmd;
        }
        #endregion


        /// <summary>
        /// 驱动战斗逻辑更新
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="inputCmd"></param>
        /// <returns></returns>
        public int Update(float deltaTime,FsCmd inputCmd)
        {
            //float需要保证不同平台计算精准性
            int logicFrames = 0;
            var past = deltaTime;
            SubmitCmdCache.Add(inputCmd);
            #region 推进游戏逻辑帧
            Accumulator += past;
            while (Accumulator >= FrameLength)
            {
                var cmd = MergeCmdList(SubmitCmdCache);
                SubmitCmdCache.Clear();
                Accumulator -= FrameLength;
                ReplaySave?.SaveCmd(cmd);
                GameLogicFrame(cmd);
                FrameIndex++;
                LogicTime += FrameLength;
                logicFrames++;
            }
            #endregion
            return logicFrames;
        }

        #region 录像重播
        
        public int ReplayCmdIndex = 0;
        public FsCmd GetFrameReplayCmd(int logicFrame)
        {
            for (int i = ReplayCmdIndex; i < Replay.Cmds.Count; i++)
            {
                var cmd = Replay.Cmds[i];
                if (cmd.LogicFrameIndex == logicFrame)
                {
                    ReplayCmdIndex = i + 1;
                    return cmd;
                }
            }
            return null;
        }
        /// <summary>
        /// 重播的方式驱动战斗逻辑更新
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public int ReplayUpdate(float deltaTime)
        {
            //整体逻辑与Update相同 主要是操作是来自记录而不是输入提供
            int logicFrames = 0;
            var past = deltaTime;
            #region 推进游戏逻辑帧
            Accumulator += past;
            while (Accumulator >= FrameLength)
            {
                FsCmd replayCmd = GetFrameReplayCmd(FrameIndex);
                Accumulator -= FrameLength;
                GameLogicFrame(replayCmd);
                FrameIndex++;
                LogicTime += FrameLength;
                logicFrames++;
            }
            #endregion
            return logicFrames;
        }
        #endregion
        
        protected virtual void GameLogicFrame(FsCmd cmd)
        {
            var p = (this, cmd);
            Entities.RefForEach(ref p, ((logic, param) =>
            {
                logic.LogicFrame(param.Item1, param.cmd);
            }));
        }

        /// <summary>
        /// 如果不为空 说明本次战斗开启了记录
        /// </summary>
        public FsBattleReplay ReplaySave { get; private set; }
        
        /// <summary>
        /// 正在使用的录像数据
        /// </summary>
        public FsBattleReplay Replay { get; private set; }
        
    }

    public class FsBattleReplay
    {
        public int Fps;
        public int Seed;
        public FsBattleStartData StartData;
        public List<FsCmd> Cmds;

        public void Init(int fps,int seed,FsBattleStartData startData)
        {
            this.Fps = fps;
            this.Seed = seed;
            this.StartData = startData;
            this.Cmds = new List<FsCmd>();
        }
        
        
        public void SaveCmd(FsCmd cmd)
        {
            Cmds.Add(cmd);
        }
        
    }
}