using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FrameSyncBattle
{

    public class FsEntityViewCreator
    {
        public Dictionary<Type, Type> ViewMap = new Dictionary<Type, Type>()
        {
            //后面考虑反射绑定
            { typeof(FsPlayerLogic),typeof(FsPlayerView) },
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
    public class FsEntityView : MonoBehaviour
    {
        private static readonly FsEntityViewCreator Creator = new();
        public FsEntityLogic Logic { get; private set; }
        public static FsEntityView Create<T>(T entityLogic) where T : FsEntityLogic, new()
        {
            var view = Creator.Create(entityLogic);
            view.Init(entityLogic);
            return view;
        }

        protected virtual void Init(FsEntityLogic entityLogic)
        {
            this.Logic = entityLogic;
            StartPosition = entityLogic.Position;
            StartEuler = entityLogic.Euler;
        }
        
        
        //记录上一逻辑帧数据 用来插值 考虑用一个简单克隆对象来记录Logic对象?
        public Vector3 StartPosition;
        public Vector3 StartEuler;
        public virtual void ViewInterpolation(FsBattleGame battleGame, float lerp)
        {
            var position = Vector3.Lerp(StartPosition, Logic.Position, lerp);
            transform.position = position;
            var euler = Vector3.Lerp(StartEuler, Logic.Euler, lerp);
            transform.eulerAngles = euler;
        }

        public virtual void PrepareLerp(FsBattleGame battleGame)
        {
            StartPosition = Logic.Position;
            StartEuler = Logic.Euler;
            ViewInterpolation(battleGame,0f);
        }
    }
}