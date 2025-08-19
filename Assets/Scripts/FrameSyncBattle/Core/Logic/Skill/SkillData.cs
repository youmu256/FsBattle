namespace FrameSyncBattle
{
    
    /// <summary>
    /// 技能静态数据 来自配置
    /// </summary>
    public class SkillData
    {
        public string Id;
        public string Name;
        public string Desc;
        public string Icon;
        public bool IsPassive;
        public bool IsAttack;
        public SkillTargetType TargetType;
        public SkillSubType SubType;
        public int CostMp = 0;
        public float CastRange = 0;
        public float CoolDown = 0;
        //技能数据分为 基础技能设置 和 技能参数设置
        //通用技能参数 施法距离 冷却
        //特殊参数 技能自身的特殊参数
        
        //AI
        public SkillAITargetRx AIRx;
        public SkillAITarget AITarget;
    }
}