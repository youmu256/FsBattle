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
            LastLogicPosition = entityLogic.Position;
            LastLogicEuler = entityLogic.Euler;
            LastLogicAnimationReq = Logic.AnimationReq;
            //Debug.Log($"view init {entityLogic.Id} - {entityLogic.TypeId}");
            //生成模型节点
            ModelRoot = new GameObject("model").transform;
            ModelRoot.SetParent(CachedTransform,false);
            TestStringData = new NoInterpolationStringData();
            TestStringData.Init(this);
            SetModelByPath(Logic.ViewModel,Logic.ViewModelScale);
            ViewInterpolation(0);
        }


        //记录上一逻辑帧数据 用来插值 考虑用一个简单克隆对象来记录Logic对象?
        public Vector3 LastLogicPosition;
        public Vector3 LastLogicEuler;
        public PlayAnimParam LastLogicAnimationReq;
        public NoInterpolationStringData TestStringData;
        
        
        public virtual void ViewInterpolation(float lerp)
        {
            var position = Vector3.Lerp(LastLogicPosition, Logic.Position, lerp);
            CachedTransform.position = position;
            var euler = Vector3.Lerp(LastLogicEuler, Logic.Euler, lerp);
            CachedTransform.eulerAngles = euler;
        }

        /// <summary>
        /// 执行逻辑帧之前 也是上一个逻辑帧插值的结束时间
        /// 上一个逻辑帧的一些非插值变化 应该在这里表现
        /// </summary>
        /// <param name="battleGame"></param>
        /// <param name="lerp"></param>
        public virtual void BeforeLogicFrame(FsBattleGame battleGame, float lerp)
        {
            //在逻辑帧执行之前 设置旧的逻辑状态作为插值起点
            LastLogicPosition = Logic.Position;
            LastLogicEuler = Logic.Euler;
            //此时该播放上一次逻辑帧的动画
            if(LastLogicAnimationReq.IsValid())
                PlayAnimation(LastLogicAnimationReq);
            LastLogicAnimationReq = Logic.AnimationReq;
            TestStringData.BeforeLogicFrame(battleGame,this);
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

        public void SetModelByPath(string model,float scale)
        {
            Debug.Log($"{this.Logic.Id} view model init : {Logic.ViewModel}");
            //TODO 后续要通过资源系统来找到对应的模型
            switch (model)
            {
                case "test_unit":
                    SetModelByPrefab(FsBattleUnity.Instance.PlayerModel,scale);
                    break;
                case "test_missile":
                    SetModelByPrefab(FsBattleUnity.Instance.CubeModel,scale);
                    break;
            }
        }
        
        public void SetModelByPrefab(GameObject prefab,float scale = 1f)
        {
            var inst = GameObject.Instantiate(prefab);
            SetModel(inst,scale);
        }


        public void SetModel(GameObject model,float scale)
        {
            if (Model != null)
            {
                GameObject.Destroy(Model.gameObject);
                Model = null;
            }
            model.transform.SetParent(ModelRoot,false);
            model.transform.localScale = Vector3.one;
            model.transform.localEulerAngles = Vector3.zero;
            model.transform.localPosition = Vector3.zero;
            model.gameObject.SetActive(true);
            Model = model.GetComponent<AnimModel>();
            ModelRoot.localScale = Vector3.one * scale;
        }

        #endregion

        #region IFsEntityView

        public void PlayAnimation(PlayAnimParam animParam)
        {
            if (Model == null) return;
            Model.PlayAnimation(animParam);
        }

        #endregion
    }

    public class NoInterpolationStringData
    {
        public string Model;
        public string ModelReq;

        public void Init(FsEntityView view)
        {
            Model = view.Logic.ViewModel;
        }
        
        public void BeforeLogicFrame(FsBattleLogic battle,FsEntityView view)
        {
            if (ModelReq != null)
            {
                //Do ModelReq
                FsDebug.Log($"Do Model Req {ModelReq}");
                ModelReq = null;
            }
            if (Model != view.Logic.ViewModel)
            {
                ModelReq = Model;
            }
            Model = view.Logic.ViewModel;
        }
    }
}