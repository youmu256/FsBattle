using UnityEngine;

namespace FrameSyncBattle
{
    public class FsBulletView : FsEntityView
    {
        protected override void Init(FsEntityLogic entityLogic)
        {
            base.Init(entityLogic);
            var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            model.transform.SetParent(CachedTransform,false);
            model.transform.localScale = Vector3.one * 0.2f;
        }
    }
}