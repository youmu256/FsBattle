using UnityEngine;

namespace FrameSyncBattle
{
    
    #region 配置
    /// <summary>
    /// 每次攻击的表现形式
    /// </summary>
    public class AttackHitData
    {
        /**打击发起时间*/
        public float HitTime;
        /**伤害倍率*/
        public float DamagePct;
        /**伤害作用范围 >0说明是AOE伤害*/
        public float DamageRange;
        /**是否为近战攻击*/
        public bool IsMelee;
        /**近战攻击命中特效*/
        public string MeleeHitFx;
        /**是否锁定目标*/
        public bool LockTarget;
        /**攻击弹道模型*/
        public string AttackModel;
        /**攻击弹道速度*/
        public float AttackFlySpeed;
        /**攻击弹道曲率*/
        public float AttackFlyArc;
        /**攻击弹道侧旋角度*/
        public float AttackFlySideSpin;
        /**攻击弹道起始点(相对于角色坐标系)*/
        public Vector3 AttackFireOffset;
    }
    
    public class AttackConfig
    {
        /**攻击动画名*/
        public string Anim;
        /**动画后缀名*/
        public string AnimSuffix;
        /**动画完成时间 影响攻击状态回复时间 AI在动画时间内是无法移动的*/
        public float AnimTime;
        /**是否忽略动画融合 无效*/
        public bool NoFade;
        /**攻击数据 一次攻击行为可能会有多次打击*/
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