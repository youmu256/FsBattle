using System;

namespace FrameSyncBattle
{
    
    public class SkillHandler :IFsEntityFrame
    {
        protected FsLinkedList<SkillBase> SkillList = new();

        public IAutoCasterAI AutoCasterAI = new SkillAICastHelper();

        public SkillCastOrder AIAutoCastCheck(FsBattleLogic battleLogic)
        {
            foreach (var skillBase in SkillList)
            {
                var order = AutoCasterAI.TryCast(battleLogic, Owner, skillBase);
                if (order!=null)
                    return order;
            }
            return null;
        }
        

        public void AddSkill(FsBattleLogic battleLogic, SkillBase skill,SkillData data)
        {
            SkillList.Add(skill);
            skill.OnInit(battleLogic,this,data);
            skill.OnAdd(battleLogic);
        }

        public void RemoveSkill(FsBattleLogic battleLogic, SkillBase skill)
        {
            bool rt = SkillList.Remove(skill);
            if(rt)
                skill.OnRemove(battleLogic);
        }

        public SkillBase Find(Func<SkillBase, bool> condition)
        {
            return SkillList.Find(condition);
        }
        public SkillBase FindById(string id)
        {
            return SkillList.Find((skill => skill.Id == id));
        }

        public bool TryCast(FsBattleLogic battleLogic, string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            var matchSkill = Find((skill => skill.Id == skillId));
            return matchSkill != null && matchSkill.TryCastAuto(battleLogic);
        }

        public bool TryCast(FsBattleLogic battleLogic, SkillSubType type)
        {
            if (type == SkillSubType.Other) return false;
            var matchSkill = Find((skill => skill.Data.SubType == type));
            return matchSkill != null && matchSkill.TryCastAuto(battleLogic);
        }


        public SkillHandler(FsUnitLogic owner)
        {
            this.Owner = owner;
        }
        
        public FsUnitLogic Owner { get; private set; }

        public void OnEntityFrame(FsBattleLogic battle, FsUnitLogic entity, float deltaTime, FsCmd cmd)
        {
            var p = (battle, cmd);
            SkillList.RefForEach(ref p, (logic, param) =>
            {
                var (fsBattleLogic, fsCmd) = param;
                logic.LogicFrame(fsBattleLogic,fsCmd);
            });
            //SkillList.ForEach((skill => { skill.LogicFrame(battle, cmd); }));
        }
    }
}