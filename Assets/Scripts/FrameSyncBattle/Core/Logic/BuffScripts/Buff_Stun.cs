namespace FrameSyncBattle
{
    public class Buff_Stun : BuffBase
    {
        public static BuffData TestData()
        {
            var data = new BuffData();
            data.FlagTags = BuffFlagTags.IsBad;
            data.TemplateKey = "buff_stun";
            data.Id = "stun";
            data.MaxCount = 1;
            data.BuffIcon = "stun_icon";
            data.CoverCheckType = BuffCoverCheckType.Key;
            return data;
        }

        public override BuffCoverOperate CoverCheck(FsUnitLogic source, FsUnitLogic target, BuffData data, AddBuffRequest request)
        {
            return BuffCoverOperate.Old_CountC_TimeBigger;
        }

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