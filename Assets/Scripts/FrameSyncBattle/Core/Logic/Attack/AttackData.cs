using UnityEngine;

namespace FrameSyncBattle
{
    
    #region 配置
    /// <summary>
    /// 每次攻击的表现形式
    /// </summary>
    public class AttackHitData
    {
        public float HitTime;
        public float DamagePct;
        public float DamageRange;//伤害范围 >0 AOE半径
        public float DamageHeight;//伤害范围 >0 高度判定
        public bool IsMelee;//是否为近战攻击
        public string MeleeHitFx;
        //下面是远程攻击的配置数据
        public bool LockTarget;
        public string AttackModel;
        public float AttackFlySpeed;
        public float AttackFlyArc;
        public float AttackFlySideSpin;
        public Vector3 AttackFireOffset;
    }
    
    public class AttackConfig
    {
        //对应动画
        public string Anim;
        public string AnimSuffix;
        public float AnimTime;
        public bool NoFade;
        //一次攻击可能会有多次命中
        public AttackHitData[] HitDatas;
        public bool IsLastHitIndex(int index)
        {
            return index > HitDatas.Length-1;
        }
        
        public AttackHitData GetCurrentHit(float time,int invalidIndex)
        {
            for (int i = invalidIndex; i < HitDatas.Length; i++)
            {
                var data = HitDatas[i];
                if (time >= data.HitTime)
                {
                    return data;
                }
            }
            return null;
        }
    }

    #endregion
}