using System;
using UnityEngine;

namespace FrameSyncBattle
{
    public class FsEntityLogic
    {
        public static int IdGenerate { get; private set; }
        public object InitData { get; private set; }
        public int Id { get; private set; }
        public string TypeId { get; protected set; }
        public Vector3 Position { get; protected set; }
        public Vector3 Euler { get; protected set; }
        
        public int Team { get; protected set; }
        
        public FsEntityLogic()
        {
            this.Id = ++IdGenerate;
        }
        
        public virtual void Init(int team,string entityTypeId,object initData)
        {
            this.Team = team;
            this.InitData = initData;
            this.TypeId = entityTypeId;
            //this.Position = Vector3.zero;
            //this.Euler = Vector3.zero;
        }
        
        public virtual void LogicFrame(FsBattleLogic battle,FsCmd cmd)
        {
            
        }
        
    }
}