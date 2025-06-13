using System;
using System.Collections.Generic;

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
        public void UpdateEntityCache(FsEntityLogic entity,bool isAdd)
        {
            if (isAdd)
            {
                if (entity is FsUnitLogic unit)
                {
                    Units.Add(unit);
                }
            }
            else
            {
                if (entity is FsUnitLogic unit)
                {
                    Units.Remove(unit);
                }
            }
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
        
        public Random Random { get; private set; }
        public FsBattleLogic(int fps,int seed)
        {
            FrameRate = fps;
            Random = new Random(seed);
        }

        protected FsLinkedList<FsEntityLogic> Entities = new();
        
        #region 逻辑帧相关
        public float LogicTime { get; private set; }
        protected float Accumulator { get; private set; }
        public int FrameIndex { get; private set; }
        public int FrameRate { get; private set; }
        public float FrameLength
        {
            get
            {
                return 1f / FrameRate;
            }
        }

        #endregion
        
        #region 操作
        /*
        public List<FsCmd> PlayerLogicCmdList = new();
        public int LogicCmdIndex = 0;
        public FsCmd GetFrameLogicCmd(int logicFrame)
        {
            for (int i = LogicCmdIndex; i < PlayerLogicCmdList.Count; i++)
            {
                var cmd = PlayerLogicCmdList[i];
                if (cmd.LogicFrameIndex == logicFrame)
                {
                    LogicCmdIndex = i + 1;
                    return cmd;
                }
            }
            return null;
        }
        */
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
        /// <param name="frameCmd"></param>
        /// <returns></returns>
        public int Update(float deltaTime,FsCmd frameCmd)
        {
            //float需要保证不同平台计算精准性
            int logicFrames = 0;
            var past = deltaTime;
            SubmitCmdCache.Add(frameCmd);
            #region 推进游戏逻辑帧
            Accumulator += past;
            while (Accumulator >= FrameLength)
            {
                var cmd = MergeCmdList(SubmitCmdCache);
                SubmitCmdCache.Clear();
                Accumulator -= FrameLength;
                GameLogicFrame(cmd);
                FrameIndex++;
                LogicTime += FrameLength;
                logicFrames++;
            }
            #endregion
            return logicFrames;
        }

        //因为逻辑帧数比渲染帧数低
        //一个游戏逻辑帧的时候可能已经有多个渲染帧操作了

        protected virtual void GameLogicFrame(FsCmd cmd)
        {
            RecordCmds.Add(cmd);
            var p = (this, cmd);
            Entities.ForEach(ref p, ((logic, param) =>
            {
                logic.LogicFrame(param.Item1, param.cmd);
            }));
        }

        public List<FsCmd> RecordCmds { get; private set; } = new();

    }
}