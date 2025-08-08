using System.Collections.Generic;

namespace FrameSyncBattle
{
    
    public class FsBattleDataHandler
    {
        protected Dictionary<string, SkillData> SkillDatas { get; private set; }
        public SkillData GetSkillData(string id)
        {
            if (SkillDatas.TryGetValue(id, out var data))
                return data;
            return null;
        }

        public void Init(FsBattleLogic battleLogic)
        {
            SkillDatas = new Dictionary<string, SkillData>();
            //装测试数据
            SkillData testSkill1 = new SkillData();
            testSkill1.Id = "test1";
            testSkill1.Desc = "test1_desc";
            testSkill1.Icon = "test1_icon";
            testSkill1.Name = "test1_name";
            testSkill1.CostMp = 100;
            testSkill1.IsAttack = false;
            testSkill1.IsPassive = false;
            testSkill1.TargetType = SkillTargetType.Unit;
            testSkill1.SubType = SkillSubType.HeroFinalSkill;
            SkillDatas.Add(testSkill1.Id,testSkill1);
        }
    }
}