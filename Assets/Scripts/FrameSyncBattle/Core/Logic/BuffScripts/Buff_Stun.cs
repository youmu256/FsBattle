namespace FrameSyncBattle
{
    public class Buff_Stun : Buff
    {
        protected override void OnAttach(FsBattleLogic battle)
        {
            base.OnAttach(battle);
            Ownner.StateFlags.Modify(FsUnitStateFlag.Attack|FsUnitStateFlag.Move|FsUnitStateFlag.Cast,false);
        }
        protected override void OnDeAttach(FsBattleLogic battle)
        {
            base.OnDeAttach(battle);
            Ownner.StateFlags.Modify(FsUnitStateFlag.Attack|FsUnitStateFlag.Move|FsUnitStateFlag.Cast,true);
        }
    }
}