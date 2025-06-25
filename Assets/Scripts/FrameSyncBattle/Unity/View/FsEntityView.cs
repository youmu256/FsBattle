using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FrameSyncBattle
{

    public class FsEntityViewCreator
    {
        public Dictionary<Type, Type> ViewMap = new()
        {
            {typeof(FsPlayerLogic), typeof(FsUnitView)},
            {typeof(FsUnitLogic), typeof(FsUnitView)},
            {typeof(FsBulletLogic), typeof(FsEntityView)},
        };

        public FsEntityView Create(FsEntityLogic logic)
        {
            var logicType = logic.GetType();
            ViewMap.TryGetValue(logicType, out var viewType);
            var go = new GameObject($"entity-{logic.Id}");
            FsEntityView view = null;
            if (viewType != null)
            {
                view = go.AddComponent(viewType) as FsEntityView;
            }
            else
            {
                Debug.LogError($"Create View Error, logic:{logicType} not find match view");
                view = go.AddComponent<FsEntityView>();
            }
            return view;
        }
    }


    /// <summary>
    /// Entity显示层基础类
    /// </summary>
    public class FsEntityView : MonoBehaviour, IFsEntityView
    {
        private static readonly FsEntityViewCreator Creator = new();


        public int Id { get; private set; }
        
        public FsEntityLogic Logic { get; private set; }

        public Transform CachedTransform { get; private set; }//调用this.transfrom会有gcalloc...
        
        public static FsEntityView Create<T>(T entityLogic) where T : FsEntityLogic, new()
        {
            var view = Creator.Create(entityLogic);
            view.Init(entityLogic);
            return view;
        }

        protected virtual void Init(FsEntityLogic entityLogic)
        {
            this.Id = entityLogic.Id;
            CachedTransform = this.transform;
            Logic = entityLogic;
            StartPosition = entityLogic.Position;
            StartEuler = entityLogic.Euler;
            //Debug.Log($"view init {entityLogic.Id} - {entityLogic.TypeId}");
            switch (entityLogic.TypeId)
            {
                case "player":
                    SetModel(FsBattleUnity.Instance.PlayerModel);
                    break;
                case "enemy":
                    SetModel(FsBattleUnity.Instance.PlayerModel);
                    break;
                case "bullet":
                    SetModel(FsBattleUnity.Instance.BulletModel,0.2f);
                    break;
            }
            ViewInterpolation(0);
        }


        //记录上一逻辑帧数据 用来插值 考虑用一个简单克隆对象来记录Logic对象?
        public Vector3 StartPosition;
        public Vector3 StartEuler;

        public virtual void ViewInterpolation(float lerp)
        {
            var position = Vector3.Lerp(StartPosition, Logic.Position, lerp);
            CachedTransform.position = position;
            var euler = Vector3.Lerp(StartEuler, Logic.Euler, lerp);
            CachedTransform.eulerAngles = euler;
        }

        public virtual void PrepareLerp(FsBattleGame battleGame, float lerp)
        {
            //在逻辑帧执行之前 设置旧的逻辑状态作为插值起点
            StartPosition = Logic.Position;
            StartEuler = Logic.Euler;
            //ViewInterpolation(battleGame,lerp);
        }

        public virtual void OnCreate(FsBattleGame battleGame)
        {
            
        }
        
        /// <summary>
        /// view对象被战斗逻辑移除时调用
        /// 清空渲染对象
        /// </summary>
        /// <param name="battleGame"></param>
        public virtual void OnRemove(FsBattleGame battleGame)
        {
            GameObject.Destroy(this.gameObject);
        }

        #region 模型相关

        public AnimModel Model { get; private set; }

        public void SetModel(GameObject prefab,float scale = 1f)
        {
            if (Model != null)
            {
                GameObject.Destroy(Model.gameObject);
                Model = null;
            }
            var inst = GameObject.Instantiate(prefab, this.transform, false);
            inst.transform.localScale = Vector3.one * scale;
            inst.transform.localEulerAngles = Vector3.zero;
            inst.transform.localPosition = Vector3.zero;
            inst.gameObject.SetActive(true);
            Model = inst.GetComponent<AnimModel>();
        }

        #endregion

        #region IFsEntityView

        public void Play(PlayAnimParam animParam)
        {
            if (Model == null) return;
            Model.PlayAnimation(animParam);
        }

        #endregion
    }
}