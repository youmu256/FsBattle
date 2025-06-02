using UnityEngine;

namespace FrameSyncBattle
{
    public class FsPlayerView : FsUnitView
    {
        protected override void Init(FsEntityLogic entityLogic)
        {
            base.Init(entityLogic);
            //GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(this.transform);
        }
    }
}