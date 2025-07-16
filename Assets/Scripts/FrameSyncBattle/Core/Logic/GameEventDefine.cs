namespace FrameSyncBattle
{
    public class GameEventDefine
    {
        
    }
    /// <summary>
    /// 准备发起攻击
    /// </summary>
    public class GE_AnyUnitPrepareAttack : GameEvent
    {
        public FsUnitLogic Source;
        public FsUnitLogic Target;
        public AttackConfig OriginAttack;
        public override void Reset()
        {
            Source = null;
            Target = null;
            OriginAttack = null;
        }
    }
}