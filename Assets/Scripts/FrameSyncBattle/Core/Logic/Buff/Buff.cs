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
    
    public class DataCoverHelper
    {
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
        public static float GetTimeCoverValue(BuffTimeCoverType coverType,float old,float remain, float v)
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

        public int GetCoverCount(int old, int v)
        {
            return DataCoverHelper.GetCountCoverValue(countCoverType, old, v);
        }
        public float GetCoverRemainTime(float old, float remain,float v)
        {
            return DataCoverHelper.GetTimeCoverValue(remainTimeCoverType, old,remain,v );
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
        public string BuffTypeKey = null;//key标记 buff类型 同一个buff代码可以当做多个buff效果，比如加速减速对应一个buff代码但是实际是2个buff。
        public string TemplateKey = null;//buff模板key
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
        public string TypeKey;//buffKey代表一个buff类型
        public string RuntimeKey;//代表运行时的key值，用来检查冲突
        //public string TemplateKey;//模板逻辑Key
        public BuffCoverCheckType CoverCheckType;
        public int Count;//层数
        public int MaxCount;//最大层数
        public float Timer;//添加时间
        public float LastTime;//持续时间
        public string BuffIcon;//buff关联的icon
        public bool IsBenefit = false;//有益

        public FsUnitLogic Source;
        public FsUnitLogic Ownner;
        public IBuffSource OtherSource;
        private bool toRemoveFlag = false;

        public void MarkToRemove()
        {
            toRemoveFlag = true;
        }
        
        public bool GetToRemoveFlag()
        {
            return toRemoveFlag;
        }

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
        
        public void InitData(BuffData data)
        {
            this.TypeKey = data.BuffTypeKey;
            this.CoverCheckType = data.CoverCheckType;
            this.MaxCount = data.MaxCount;
            this.BuffIcon = data.BuffIcon;
            this.IsBenefit = data.IsBenefit;
            OnInitData(data);
        }

        protected virtual void OnInitData(BuffData data)
        {
            
        }

        public int GetValidCount(int cnt)
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

        public void Refresh(BuffData data,int changeCount){
            this.Timer = 0;
            int realAddCnt = 0;
            if(changeCount !=0){
                int oriCount = Count;
                Count = GetValidCount(Count+ changeCount);
                realAddCnt = Count - oriCount;
            }
            OnRefresh(data,realAddCnt);
        }
        public void OnRemove()
        {
            OnDeAttach();
        }

        protected virtual void OnRefresh(BuffData data,int changeCnt)
        {
            
        }

        public virtual void OnAttach()
        {
            
        }

        public virtual void OnDeAttach()
        {
            
        }

        protected virtual void OnFrame(float deltaTime)
        {
            
        }
        public virtual BuffCoverOperate CoverCheck(FsUnitLogic source,FsUnitLogic target, BuffData data,AddBuffRequest request){
            //data是要用来创建新buff的data 如果要比较来选择返回结果的话，对data进行比较
            return BuffCoverOperate.New_CountC_TimeN;
        }
        public void Frame(float deltaTime)
        {
            OnFrame(deltaTime);
            if (GetToRemoveFlag() == false && IsNeedToRemove())
            {
                MarkToRemove();
            }
            Timer += deltaTime;
        }

        public void SetFirstAttach(string buffKey,IBuffSource other,FsUnitLogic source,FsUnitLogic target,BuffData data,int count,float lastTime)
        {
            this.RuntimeKey = buffKey;
            InitData(data);
            this.OtherSource = other;
            SetAttach(source,target,data,count,lastTime);
        }
        public void SetAttach(FsUnitLogic source,FsUnitLogic target,BuffData data,int count,float lastTime)
        {
            this.Count = count;
            this.Source = source;
            this.Ownner = target;
            this.Timer = 0;
            this.LastTime = lastTime;
            OnAttach();
        }
        
        public void SetReAttach(FsUnitLogic source,FsUnitLogic target,BuffData data,int count,float lastTime)
        {
            SetDeAttach();
            int cnt = GetValidCount(count);
            SetAttach(source,target,data,cnt,lastTime);
        }
        public void SetDeAttach(){
            OnDeAttach();
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
            return BuildRuntimeKey(typeKey, sourceId + "", otherKey);
        }
        private static string BuildRuntimeKey(string typeKey, string sourceKey, string other){
            StringBuilder sb = new StringBuilder();
            sb.Append(typeKey).Append('-').Append(sourceKey).Append('-').Append(other);
            return sb.ToString();
        }
        #endregion
    }
}