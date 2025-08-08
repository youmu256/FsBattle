using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsBattleAssets
    {
        /*
         * 关于战斗内资源
         * 战斗内资源应该都使用同步加载 异步加载会让事情变得更加复杂
         *
         * 战斗内用到的资源应该提前预加载
         * 比如特效，角色模型等
         *
         * 表现层每逻辑帧都会去处理逻辑层的
         * 
         */
        
        protected Dictionary<string, AnimModel> AnimModelsCache = new();


        public void Init()
        {
            //TODO Build Cache
        }

        public AnimModel CreateModel(string assetPath)
        {
            if (AnimModelsCache.TryGetValue(assetPath, out var model))
            {
                var copy = Object.Instantiate(model);
                return copy;
            }
            return null;
        }
    }
}