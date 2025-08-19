using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{

    
    
    /// <summary>
    /// 一个Buff有多个标签状态
    /// 像一些Buff也允许又属于有益有属于有害 但是通常设计上应该避免
    /// </summary>
    [Flags]
    public enum BuffFlagTags : ulong
    {
        None = 0,
        /**不可被其他效果清除*/
        UnCleanable = 1<<0,
        /**有益的*/
        IsGood = 1<<1,
        /**有害的*/
        IsBad =1<<2,
    }
    
    /// <summary>
    /// Buff静态配置
    /// </summary>
    public class BuffData
    {
        /**Buff实例类型Key 一个代码模板可以有多个变体类型实例*/
        public string Id = null;
        /**代码模板Key*/
        public string TemplateKey = null;
        public BuffCoverCheckType CoverCheckType = BuffCoverCheckType.Key;
        public int MaxCount = 1;
        public string BuffIcon;
        public BuffFlagTags FlagTags= BuffFlagTags.None;
        protected Dictionary<string,string> CustomParamMap = new();//额外数据
        
        
        public BuffData SetValue(string k, int v)
        {
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
}