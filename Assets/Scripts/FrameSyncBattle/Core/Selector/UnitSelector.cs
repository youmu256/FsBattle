using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public interface IUnitSelectorFilter
    {
        bool DoFilter(FsUnitLogic unit);
    }

    //将用到的匿名类都手动写出来 然后就可以用对象池管理起来
    
    public class UnitInRangeFilter : IUnitSelectorFilter
    {
        public float Range;
        public Vector3 Position;
        public bool DoFilter(FsUnitLogic unit)
        {
            var p = unit.Position - Position;
            return p.magnitude <= Range;
        }
    }
    public class CustomFilter : IUnitSelectorFilter
    {
        public Func<FsUnitLogic, bool> Condition { get; private set; }

        public CustomFilter(Func<FsUnitLogic, bool> condition)
        {
            this.Condition = condition;
        }

        public bool DoFilter(FsUnitLogic unit)
        {
            return Condition.Invoke(unit);
        }
    }
    
    public class UnitSelector
    {
        public List<Func<FsUnitLogic, bool>> Filters = new();
        public UnitSelector InRange(Vector3 position, float range)
        {
            Filters.Add(new UnitInRangeFilter(){Position = position,Range = range}.DoFilter);
            return this;
        }

        public UnitSelector Condition(Func<FsUnitLogic, bool> condition)
        {
            Filters.Add(condition);
            return this;
        }

        public IComparer<FsUnitLogic> Comparer { get; private set; }
        
        public UnitSelector Sort(IComparer<FsUnitLogic> comparer)
        {
            this.Comparer = comparer;
            return this;
        }

        public void SelectTo(FsBattleLogic battleLogic, List<FsUnitLogic> results)
        {
            var units = battleLogic.EntityService.Units;
            foreach (var unit in units)
            {
                bool isValidUnit = true;
                foreach (var filter in Filters)
                {
                    bool valid = filter.Invoke(unit);
                    if (valid == false)
                    {
                        isValidUnit = false;
                        break;
                    }
                }

                if (isValidUnit)
                {
                    results.Add(unit);
                }
            }
            if(Comparer!=null)
                results.Sort(Comparer);
        }
        public List<FsUnitLogic> Select(FsBattleLogic battleLogic)
        {
            List<FsUnitLogic> rets = new List<FsUnitLogic>();
            SelectTo(battleLogic, rets);
            return rets;
        }
    }

    public class TestClass
    {
        public void T(FsBattleLogic battle)
        {
            List<FsUnitLogic> targets = new List<FsUnitLogic>();
            new UnitSelector().InRange(Vector3.zero, 100).SelectTo(battle, targets);
            foreach (var target in targets)
            {
                //do damage etc..
            }
            //匿名类很多 频繁用这种办法选取new的对象会很多..
            
        }
    }

}