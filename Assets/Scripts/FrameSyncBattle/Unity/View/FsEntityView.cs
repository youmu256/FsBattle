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
            {typeof(FsPlayerControlUnitLogic), typeof(FsEntityView)},
            {typeof(FsUnitLogic), typeof(FsEntityView)},
            {typeof(FsBulletLogic), typeof(FsEntityView)},
            {typeof(FsMissileLogic), typeof(FsEntityView)},
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
    /// 包含位置 旋转 动画展示
    /// </summary>
    public class FsEntityView : MonoBehaviour,IFsEntityView
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
            StartAnimation = Logic.AnimationReq;
            //Debug.Log($"view init {entityLogic.Id} - {entityLogic.TypeId}");
            ModelRoot = new GameObject("model").transform;
            ModelRoot.SetParent(CachedTransform,false);
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
                case "missile":
                    SetModel(FsBattleUnity.Instance.BulletModel,0.2f);
                    break;
            }
            ViewInterpolation(0);
        }


        //记录上一逻辑帧数据 用来插值 考虑用一个简单克隆对象来记录Logic对象?
        public Vector3 StartPosition;
        public Vector3 StartEuler;
        public PlayAnimParam StartAnimation;
        
        public virtual void ViewInterpolation(float lerp)
        {
            var position = Vector3.Lerp(StartPosition, Logic.Position, lerp);
            CachedTransform.position = position;
            var euler = Vector3.Lerp(StartEuler, Logic.Euler, lerp);
            CachedTransform.eulerAngles = euler;
        }

        public virtual void BeforeLogicFrame(FsBattleGame battleGame, float lerp)
        {
            //在逻辑帧执行之前 设置旧的逻辑状态作为插值起点
            StartPosition = Logic.Position;
            StartEuler = Logic.Euler;
            //此时该播放上一次逻辑帧的动画
            if(StartAnimation.Animation!=null)
                PlayAnimation(StartAnimation);
            StartAnimation = Logic.AnimationReq;
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

        public Transform ModelRoot { get; private set; }
        
        public AnimModel Model { get; private set; }

        public void SetModel(GameObject prefab,float scale = 1f)
        {
            if (Model != null)
            {
                GameObject.Destroy(Model.gameObject);
                Model = null;
            }
            var inst = GameObject.Instantiate(prefab, this.ModelRoot, false);
            inst.transform.localScale = Vector3.one;
            inst.transform.localEulerAngles = Vector3.zero;
            inst.transform.localPosition = Vector3.zero;
            inst.gameObject.SetActive(true);
            Model = inst.GetComponent<AnimModel>();
            ModelRoot.localScale = Vector3.one * scale;
        }

        #endregion

        #region IFsEntityView

        public void PlayAnimation(PlayAnimParam animParam)
        {
            if (Model == null) return;
            /*
            if (Logic.Team == FsBattleLogic.EnemyTeam)
            {
                Debug.Log($"{this.name} : play {animParam.Animation} with speed {animParam.Speed}");
            }
            */
            Model.PlayAnimation(animParam);
        }

        #endregion
    }
}