using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FrameSyncBattle
{
    public enum BuffCoverCheckType{
        Key,//Key相同判定为Cover
        Key_Source,//Key和Source相同才判定为Cover
        Key_Source_Other,//Key,Source,Other都相同时
        Independent,//完全独立 还不支持
    }
    public enum BuffCoverType
    {
        Ignore,
        UseOld,
        UseNew,
    }
    public enum BuffCountCoverType
    {
        UseOld,
        UseNew,
        Combine,
        UseBigger,
        UseSmaller,
    }
    public enum BuffTimeCoverType
    {
        UseRemain,
        UseOld,
        UseNew,
        CombineRemainNew,
        CombineOldNew,
        UseRemainNewBigger,
        UseRemainNewSmaller,
        UseOldNewBigger,
        UseOldNewSmaller,
    }
    
    public struct BuffCoverOperate
    {
        /// <summary>
        /// 啥都不做，保留老的
        /// </summary>
        public static BuffCoverOperate IGNORE = new BuffCoverOperate() {buffCoverType = BuffCoverType.Ignore};
        /// <summary>
        /// 使用新BUFF，累加层数，时间使用新
        /// </summary>
        public static BuffCoverOperate New_CountC_TimeN = new BuffCoverOperate() {buffCoverType = BuffCoverType.UseNew,remainTimeCoverType = BuffTimeCoverType.UseNew,countCoverType = BuffCountCoverType.Combine};
        /// <summary>
        /// 使用老BUFF，累加层数，累加剩余时间与新时间
        /// </summary>
        public static BuffCoverOperate Old_CountC_TimeCRN = new BuffCoverOperate(){buffCoverType = BuffCoverType.UseOld,remainTimeCoverType = BuffTimeCoverType.CombineRemainNew,countCoverType = BuffCountCoverType.Combine};
        /// <summary>
        /// 使用老BUFF，累加层数，使用大的时间
        /// </summary>
        public static BuffCoverOperate Old_CountC_TimeBigger = new BuffCoverOperate(){buffCoverType = BuffCoverType.UseOld,remainTimeCoverType = BuffTimeCoverType.UseRemainNewBigger,countCoverType = BuffCountCoverType.Combine};

        public BuffCountCoverType countCoverType;
        public BuffTimeCoverType remainTimeCoverType;
        public BuffCoverType buffCoverType;


        #region Cover

        
        public static int GetCountCoverValue(BuffCountCoverType coverType,int old, int v)
        {
            int ret;
            switch (coverType)
            {
                case BuffCountCoverType.UseOld:
                    ret = old;
                    break;
                case BuffCountCoverType.UseNew:
                    ret = v;
                    break;
                case BuffCountCoverType.Combine:
                    ret = old + v;
                    break;
                case BuffCountCoverType.UseBigger:
                    ret = Mathf.Max(old, v);
                    break;
                case BuffCountCoverType.UseSmaller:
                    ret = Mathf.Min(old, v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return ret;
        }

        private static float GetTimeCoverValue(BuffTimeCoverType coverType,float old,float remain, float v)
        {
            float ret;
            switch (coverType)
            {
                case BuffTimeCoverType.UseOld:
                    ret = old;
                    break;
                case BuffTimeCoverType.UseRemain:
                    ret = remain;
                    break;
                case BuffTimeCoverType.UseNew:
                    ret = v;
                    break;
                case BuffTimeCoverType.CombineOldNew:
                    ret = old + v;
                    break;
                case BuffTimeCoverType.CombineRemainNew:
                    ret = remain + v;
                    break;
                case BuffTimeCoverType.UseOldNewBigger:
                    ret = Mathf.Max(old, v);
                    break;
                case BuffTimeCoverType.UseOldNewSmaller:
                    ret = Mathf.Min(old, v);
                    break;
                case BuffTimeCoverType.UseRemainNewBigger:
                    ret = Mathf.Max(remain, v);
                    break;
                case BuffTimeCoverType.UseRemainNewSmaller:
                    ret = Mathf.Min(remain, v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return ret;
        }

        #endregion
        
        public int GetCoverCount(int old, int v)
        {
            return GetCountCoverValue(countCoverType, old, v);
        }
        public float GetCoverRemainTime(float old, float remain,float v)
        {
            return GetTimeCoverValue(remainTimeCoverType, old,remain,v );
        }
    }

    /// <summary>
    /// 提供BuffKey
    /// </summary>
    public interface IBuffSource
    {
        string GetPartBuffKey();
    }
    public class AddBuffRequest
    {
        public FsUnitLogic Source;
        public FsUnitLogic Target;
        public IBuffSource OtherSource;
        public BuffData Data;
        public int AddCount;
        public float LastTime;
        
        public void Reset()
        {
            Source = null;
            OtherSource = null;
            Target = null;
            Data = null;
            AddCount = 0;
            LastTime = 0;
        }

        private static readonly Stack<AddBuffRequest> RequestPool = new Stack<AddBuffRequest>();

        public static AddBuffRequest Allocate(){
            AddBuffRequest request = null;
            if(RequestPool.Count == 0){
                request = new AddBuffRequest();
            }else{
                request = RequestPool.Pop();
            }
            return request;
        }
        public static void Recycle(AddBuffRequest request){
            request.Reset();
            RequestPool.Push(request);
        }
    }

    
    public class BuffData
    {
        /**代码模板Key*/
        public string TemplateKey = null;
        /**Buff实例类型Key 一个代码模板可以有多个变体类型实例*/
        public string BuffTypeKey = null;
        public BuffCoverCheckType CoverCheckType = BuffCoverCheckType.Key;
        public int MaxCount = 1;
        //public int AddCount = 1;
        public string BuffIcon;//关联的icon
        public bool IsBenefit = false;//是否有益
        protected Dictionary<string,string> CustomParamMap = new Dictionary<string,string>();//额外数据
        
        
        public BuffData SetValue(string k, int v){
            CustomParamMap.Add(k,""+v);
            return this;
        }
        public BuffData SetValue(string k, float v){
            CustomParamMap.Add(k,""+v);
            return this;
        }
        public BuffData SetValue(string k, bool v){
            CustomParamMap.Add(k,""+v);
            return this;
        }
        public BuffData SetValue(string k, string v){
            CustomParamMap.Add(k,v);
            return this;
        }
        public int GetInt(string k){
            if(!CustomParamMap.ContainsKey(k))return 0;
            string v = CustomParamMap[k];
            return int.Parse(v);
        }
        public float GetFloat(string k){
            if(!CustomParamMap.ContainsKey(k))return 0;
            string v = CustomParamMap[k];
            return float.Parse(v);
        }
        public bool GetBoolean(string k){
            if(!CustomParamMap.ContainsKey(k))return false;
            string v = CustomParamMap[k];
            return bool.Parse(v);
        }
        public string GetString(string k){
            if(!CustomParamMap.ContainsKey(k))return null;
            string v = CustomParamMap[k];
            return v;
        }
    }
    
    public class Buff
    {
        public const int BuffPropertyLevel = 1;
        
        /**Buff实例类型*/
        public string TypeKey { get; set; }
        /**运行时身份Key用来检查冲突*/
        public string RuntimeKey { get; set; }
        public BuffCoverCheckType CoverCheckType { get; set; }
        public int Count { get; set; }
        public int MaxCount { get; set; }
        public float Timer { get; set; }
        public float LastTime { get; set; }
        public string BuffIcon { get; set; }
        public bool IsBenefit { get; set; }

        public FsUnitLogic Source;
        public FsUnitLogic Ownner;
        public IBuffSource OtherSource;
        
        protected virtual bool IsNeedToRemove()
        {
            if (Timer >= LastTime || Ownner.IsDead) return true;
            return false;
        }

        public float GetRemainTime()
        {
            var remain =  LastTime - Timer;
            if (remain < 0) remain = 0;
            return remain;
        }
        

        private int GetValidCount(int cnt)
        {
            if (cnt > this.MaxCount && MaxCount > 0)
            {
                cnt = MaxCount;
            }
            if (cnt < 1)
            {
                cnt = 1;
            }
            return cnt;
        }

        protected virtual void OnInitData(FsBattleLogic battle,BuffData data)
        {
            
        }
        
        protected virtual void OnAttach(FsBattleLogic battle)
        {
            
        }

        protected virtual void OnRefresh(FsBattleLogic battle,BuffData data,int changeCnt)
        {
            
        }

        protected virtual void OnDeAttach(FsBattleLogic battle)
        {
            
        }

        protected virtual void OnFrame(FsBattleLogic battle, float deltaTime)
        {

        }

        public virtual BuffCoverOperate CoverCheck(FsUnitLogic source,FsUnitLogic target, BuffData data,AddBuffRequest request){
            //data是要用来创建新buff的data 如果要比较来选择返回结果的话，对data进行比较
            return BuffCoverOperate.New_CountC_TimeN;
        }

        public void LogicFrame(FsBattleLogic battle, FsCmd cmd)
        {
            var deltaTime = battle.FrameLength;
            OnFrame(battle, deltaTime);
            Timer += deltaTime;
        }


        /// <summary>
        /// 初始化应用data相关数据
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="data"></param>
        private void InitData(FsBattleLogic battle,BuffData data)
        {
            this.TypeKey = data.BuffTypeKey;
            this.CoverCheckType = data.CoverCheckType;
            this.MaxCount = data.MaxCount;
            this.BuffIcon = data.BuffIcon;
            this.IsBenefit = data.IsBenefit;
            OnInitData(battle,data);
        }

        /// <summary>
        /// Buff冲突不为忽略切保留旧Buff时 一般关注层数的变化
        /// Buff计时器会重置
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="data"></param>
        /// <param name="changeCount"></param>
        public void Refresh(FsBattleLogic battle, BuffData data, int changeCount)
        {
            this.Timer = 0;
            int realAddCnt = 0;
            if (changeCount != 0)
            {
                int oriCount = Count;
                Count = GetValidCount(Count + changeCount);
                realAddCnt = Count - oriCount;
            }

            OnRefresh(battle, data, realAddCnt);
        }

        /// <summary>
        /// Buff对象被附加时
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="buffKey"></param>
        /// <param name="other"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="lastTime"></param>
        public void SetAttach(FsBattleLogic battle,string buffKey,IBuffSource other,FsUnitLogic source,FsUnitLogic target,BuffData data,int count,float lastTime)
        {
            this.RuntimeKey = buffKey;
            InitData(battle,data);
            this.Count = count;
            this.Source = source;
            this.OtherSource = other;
            this.Ownner = target;
            this.Timer = 0;
            this.LastTime = lastTime;
            OnAttach(battle);
        }
        
        /// <summary>
        /// Buff被去除附加时
        /// </summary>
        public void SetDeAttach(FsBattleLogic battle){
            OnDeAttach(battle);
        }

        #region RuntimeKey

        public static string GetBuffRuntimeKey(AddBuffRequest request){
            switch (request.Data.CoverCheckType){
                case BuffCoverCheckType.Key:
                    return GetBuffRuntimeKey(request.Data.BuffTypeKey,null,null);
                case BuffCoverCheckType.Key_Source:
                    return GetBuffRuntimeKey(request.Data.BuffTypeKey, request.Source, null);
                case BuffCoverCheckType.Key_Source_Other:
                    return GetBuffRuntimeKey(request.Data.BuffTypeKey, request.Source, request.OtherSource);
                case BuffCoverCheckType.Independent:
                    break;
            }
            return null;
        }
        
        public static string GetBuffRuntimeKey(Buff buff){
            switch (buff.CoverCheckType){
                case BuffCoverCheckType.Key:
                    return GetBuffRuntimeKey(buff.TypeKey,null,null);
                case BuffCoverCheckType.Key_Source:
                    return GetBuffRuntimeKey(buff.TypeKey, buff.Source, null);
                case BuffCoverCheckType.Key_Source_Other:
                    return GetBuffRuntimeKey(buff.TypeKey, buff.Source, buff.OtherSource);
                case BuffCoverCheckType.Independent:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 计算出buff的运行时key
        /// key = type-source-other
        /// </summary>
        /// <param name="typeKey"></param>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static string GetBuffRuntimeKey(string typeKey, FsUnitLogic source, IBuffSource other)
        {
            string sourceId = source != null ? source.Id.ToString() : "n";
            string otherKey = other != null ? other.GetPartBuffKey() : "n";
            return BuildRuntimeKey(typeKey, sourceId, otherKey);
        }
        private static string BuildRuntimeKey(string typeKey, string sourceKey, string other){
            StringBuilder sb = new StringBuilder();
            sb.Append(typeKey).Append('-').Append(sourceKey).Append('-').Append(other);
            return sb.ToString();
        }
        #endregion
    }
}