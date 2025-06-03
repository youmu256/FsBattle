using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsPlayerView : FsUnitView
    {
        protected override void Init(FsEntityLogic entityLogic)
        {
            base.Init(entityLogic);
        }

        public override void ViewInterpolation(FsBattleGame battleGame, float lerp)
        {
            base.ViewInterpolation(battleGame, lerp);
        }
    }
}