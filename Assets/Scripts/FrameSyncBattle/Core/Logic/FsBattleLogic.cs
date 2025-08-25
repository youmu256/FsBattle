using System;
using System.Collections.Generic;
using System.Text;
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

    public enum RelationShip
    {
        Friend,
        Enemy,
        Neutral,
    }

    public class FsEntityService
    {
        public FsEntityService(FsBattleLogic battle)
        {
            this.RefBattle = battle;
        }
        
        public FsBattleLogic RefBattle { get; private set; }

        //小心被错误操作
        public List<FsUnitLogic> Units { get; private set; }= new();

        public Dictionary<int, FsEntityLogic> EntitiesMap = new();

        public void UpdateEntityCache(FsEntityLogic entity, bool isAdd)
        {
            if (isAdd)
            {
                EntitiesMap.Add(entity.Id, entity);
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
        
        public ICollection<FsUnitLogic> CollectUnits(ICollection<FsUnitLogic> container, Func<FsBattleLogic, FsUnitLogic, bool> filter = null)
        {
            if (filter != null)
            {
                foreach (var unit in Units)
                {
                    if (filter.Invoke(RefBattle, unit) == false) continue;
                    container.Add(unit);
                }
            }
            else
            {
                foreach (var unit in Units)
                {
                    container.Add(unit);
                }
            }
            return container;
        }
        
        public ICollection<FsUnitLogic> CollectUnitsInPositionRange2D(ICollection<FsUnitLogic> container, Vector3 position, float range,
            Func<FsBattleLogic, FsUnitLogic, bool> filter)
        {
            TempList.Clear();
            var aoiList = TempList;
            foreach (var unit in Units)
            {
                if (DistanceUtils.DistanceBetween2D(unit, position, true) <= range)
                {
                    aoiList.Add(unit);
                }
            }

            foreach (var unit in aoiList)
            {
                if (filter.Invoke(RefBattle, unit) == false) continue;
                container.Add(unit);
            }

            TempList.Clear();
            return container;
        }


        /// <summary>
        /// 目标对象是否能被作为AI目标
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsEntityValidTobeTargeted(FsUnitLogic source, FsUnitLogic target)
        {
            if (source == null) return false;
            if (target == null) return false;
            if (target.IsDead || target.IsRemoved) return false;
            return true;
        }

        #region 获取敌对关系

        public RelationShip GetRelationship(FsUnitLogic source, FsUnitLogic target)
        {
            return GetRelationship(source.Team, target.Team);
        }

        public RelationShip GetRelationship(int source, int target)
        {
            if (IsFriend(source, target)) return RelationShip.Friend;
            if (IsEnemy(source, target)) return RelationShip.Enemy;
            if (IsNeutral(source, target)) return RelationShip.Neutral;
            return 0;
        }

        public bool IsFriend(int sourceTeam, int targetTeam)
        {
            return sourceTeam == targetTeam;
        }

        public bool IsEnemy(int sourceTeam, int targetTeam)
        {
            return sourceTeam != targetTeam;
        }

        public bool IsNeutral(int sourceTeam, int targetTeam)
        {
            return sourceTeam == -1 || targetTeam == -1;
        }

        public bool IsFriend(int sourceTeam, FsUnitLogic target)
        {
            return IsFriend(sourceTeam, target.Team);
        }

        public bool IsEnemy(int sourceTeam, FsUnitLogic target)
        {
            return IsEnemy(sourceTeam, target.Team);
        }

        public bool IsNeutral(int sourceTeam, FsUnitLogic target)
        {
            return IsNeutral(sourceTeam, target.Team);
        }

        public bool IsFriend(FsUnitLogic source, FsUnitLogic target)
        {
            return IsFriend(source.Team, target.Team);
        }

        public bool IsEnemy(FsUnitLogic source, FsUnitLogic target)
        {
            return IsEnemy(source.Team, target.Team);
        }

        public bool IsNeutral(FsUnitLogic source, FsUnitLogic target)
        {
            return IsNeutral(source.Team, target.Team);
        }

        #endregion

    }

    public enum FsBattlePlayState
    {
        WaitStart,
        Play,
        Pause,
        End,
    }


    public partial class FsBattleLogic
    {
        public string GetGameStateMsg()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($">>>>>>>>>>GameState  GameFrame:{FrameIndex}<<<<<<<<<<");
            sb.AppendLine($"===Units===");
            foreach (var entity in EntityService.CollectUnits(new List<FsUnitLogic>()))
            {
                sb.AppendLine(entity.DebugMsg());
            }
            sb.AppendLine($"===Units===");
            return sb.ToString();
        }
    }
    
    public partial class FsBattleLogic
    {
        public FsEntityService EntityService { get; private set; }

        public int EntityIdGenerator { get; private set; }
        //public GameEventHandler EventHandler { get; private set; } = new GameEventHandler();

        public FsEntityLogic CreateFxEntity(Vector3 pos,Vector3 euler)
        {
            var entity = AddEntity<FsEntityLogic>(0,FsEntityType.Fx,pos,euler,null);
            return entity;
        }
        
        public T AddEntity<T>(int team,FsEntityType entityType,Vector3 pos,Vector3 euler, object initData) where T : FsEntityLogic, new()
        {
            EntityIdGenerator++;//ID增长
            var entity = new T();
            entity.SetPosition(pos).SetEuler(euler);
            entity.Init(this, team,entityType, initData);
            entity.OnCreate(this);
            Entities.Add(entity);
            EntityService.UpdateEntityCache(entity,true);
            return entity;
        }

        public void RemoveEntity(FsEntityLogic entity)
        {
            entity.OnRemove(this);
            Entities.Remove(entity);
            EntityService.UpdateEntityCache(entity,false);
        }

    }

    public partial class FsBattleLogic
    {
        public const int PlayerTeam = 1;
        public const int EnemyTeam = 2;
        public bool IsReplayMode { get; private set; } = false;
        public Random RandomGen { get; private set; }
        public FsAutoBattleAI AutoBattleAI { get; private set; }
        public FsBattleDataService DataService { get; private set; }
        public FsBattlePlayState PlayState { get; private set; }
        
        private void CommonInit()
        {
            WinTeam = 0;
            PlayState = FsBattlePlayState.WaitStart;
            DataService = new FsBattleDataService();
            DataService.Init(this);
            AutoBattleAI = new FsAutoBattleAI();
            EntityService = new FsEntityService(this);
        }
        
        public void Init(int fps,int seed,FsBattleStartData startData)
        {
            CommonInit();
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
            CommonInit();
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
            foreach (var startUnitData in startData.PlayerTeamUnits)
            {
                var ap = this.DataService.GetTeamIndexPoint(PlayerTeam, startUnitData.InitPosId);
                var entity = this.AddEntity<FsUnitLogic>(PlayerTeam, FsEntityType.Unit,ap.Position,ap.Euler, startUnitData.UnitInitData);
                entity.SetToAnglePoint(this.DataService.GetTeamIndexPoint(PlayerTeam, startUnitData.InitPosId));
            }
            foreach (var startUnitData in startData.EnemyTeamUnits)
            {
                var ap = this.DataService.GetTeamIndexPoint(EnemyTeam, startUnitData.InitPosId);
                var entity = this.AddEntity<FsUnitLogic>(EnemyTeam, FsEntityType.Unit,ap.Position,ap.Euler, startUnitData.UnitInitData);
            }
        }
        

        public void StartBattle()
        {
            PlayState = FsBattlePlayState.Play;
        }

        public void Pause()
        {
            if (PlayState == FsBattlePlayState.Play)
                PlayState = FsBattlePlayState.Pause;
        }

        public void Resume()
        {
            if (PlayState == FsBattlePlayState.Pause)
                PlayState = FsBattlePlayState.Play;
        }
        
        public void EndBattle()
        {
            if (PlayState == FsBattlePlayState.End) return;
            PlayState = FsBattlePlayState.End;
            FsDebug.Log(this.GetGameStateMsg());
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
                bool isReplayEnd = replayCmd == null;
                if (isReplayEnd)
                {
                    FsDebug.Log("REPLAY END!");
                    EndBattle();
                    break;
                }
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

        /**胜负裁判 后续考虑抽象OOP*/
        public FsBattleWinLostJudge WinLostJudge { get; set; } = new FsBattleWinLostJudge();

        public int WinTeam { get; private set; }
        
        protected virtual void GameLogicFrame(FsCmd cmd)
        {
            var p = (this, cmd);
            Entities.RefForEach(ref p, ((logic, param) =>
            {
                logic.LogicFrame(param.Item1, param.cmd);
            }));

            if (WinLostJudge != null)
            {
                WinTeam = WinLostJudge.GameLogicFrame(this);
                if (WinTeam > 0)
                {
                    EndBattle();
                }
            }
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

    public class FsBattleWinLostJudge
    {
        public int GameLogicFrame(FsBattleLogic battle)
        {
            int pcount = 0;
            int ecount = 0;
            foreach (var unit in battle.EntityService.Units)
            {
                if (unit.Team == FsBattleLogic.PlayerTeam)
                    pcount++;
                if (unit.Team == FsBattleLogic.EnemyTeam)
                    ecount++;
            }
            if (pcount == 0)
            {
                return FsBattleLogic.EnemyTeam;
            }
            if (ecount == 0)
            {
                return FsBattleLogic.PlayerTeam;
            }
            return 0;
        }
    }
}