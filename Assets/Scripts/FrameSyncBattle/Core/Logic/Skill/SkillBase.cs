using System;

namespace FrameSyncBattle
{

    public class SkillHandler
    {
        protected FsLinkedList<SkillBase> SkillList = new();

        public void AddSkill(FsBattleLogic battleLogic, SkillBase skill)
        {
            SkillList.Add(skill);
            skill.OnAdd(battleLogic, this);
        }

        public void RemoveSkill(FsBattleLogic battleLogic, SkillBase skill)
        {
            bool rt = SkillList.Remove(skill);
            if(rt)
                skill.OnRemove(battleLogic, this);
        }

        public SkillBase Find(Func<SkillBase, bool> condition)
        {
            return SkillList.Find(condition);
        }

        public bool TryCast(FsBattleLogic battleLogic, string skillId)
        {
            var matchSkill = Find((skill => skill.Id == skillId));
            return matchSkill != null && matchSkill.TryCast(battleLogic, this);
        }

        public FsUnitLogic Owner;

        public void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            SkillList.ForEach((skill => { skill.LogicFrame(battle, this, cmd); }));
        }
    }

    public class SkillData
    {
        public string Id;
        public string Name;
        public string Desc;
        public string Icon;
        //public bool IsUnique;
        public bool IsPassive;
        public bool IsAttack;
    }

    public enum SkillFlow
    {
        None,
        StartCast,
        StartEffect,
        Affecting,
        EndEffect,
        EndCast,
        Finish,
    }
    
    public class SkillBase
    {
        public string Id => Data.Id;
        public SkillData Data { get; private set; }

        public SkillFlow State { get; protected set; }

        public virtual void OnInit(FsBattleLogic battle, SkillHandler handler)
        {
            
        }
        
        public virtual void OnAdd(FsBattleLogic battle, SkillHandler handler)
        {

        }

        public virtual void OnRemove(FsBattleLogic battle, SkillHandler handler)
        {

        }

        public virtual void Stop(FsBattleLogic battle,SkillHandler handler)
        {
            if (State == SkillFlow.None || State == SkillFlow.Finish) return;
            State = SkillFlow.None;
            OnChangeFlowState(battle, handler, null);
        }
        
        public virtual bool TryCast(FsBattleLogic battle, SkillHandler handler)
        {
            bool result = false;
            //检查有效目标 检查冷却等等

            if (result)
                State = SkillFlow.StartCast;
            return result;
        }

        public virtual void LogicFrame(FsBattleLogic battle, SkillHandler handler, FsCmd cmd)
        {
            //change state
            while (true)
            {
                var pre = State;
                var next = FlowStateFrame(battle, handler, cmd);
                if (pre == next)
                    break;
                State = next;
                OnChangeFlowState(battle, handler, cmd);
            }
        }

        protected virtual void OnChangeFlowState(FsBattleLogic battle, SkillHandler handler, FsCmd cmd)
        {
            
        }
        
        protected virtual SkillFlow FlowStateFrame(FsBattleLogic battle, SkillHandler handler, FsCmd cmd)
        {
            return State;
        }
        
    }

    public class AttackBase : SkillBase
    {
        //普通攻击技能不受沉默影响 受缴械影响
        //冷却与技能不同 攻速加成会加快整个流程速度
    }

}