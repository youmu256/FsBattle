using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public interface IUnitSelectorFilter
    {
        bool DoFilter(FsUnitLogic unit);
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
        public List<IUnitSelectorFilter> Filters = new();

        public UnitSelector InRange(Vector3 position, float range)
        {
            Filters.Add(new CustomFilter((unit =>
            {
                var p = unit.Position - position;
                return p.magnitude <= range;
            })));
            return this;
        }

        public void Fill(FsBattleLogic battleLogic, List<FsUnitLogic> results)
        {
            var units = battleLogic.EntityService.UnitEntitiesCache;
            foreach (var unit in units)
            {
                bool isValidUnit = true;
                foreach (var filter in Filters)
                {
                    bool valid = filter.DoFilter(unit);
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
        }
        public UnitSelector Custom(CustomFilter filter)
        {
            Filters.Add(filter);
            return this;
        }
        public List<FsUnitLogic> Get(FsBattleLogic battleLogic)
        {
            List<FsUnitLogic> rets = new List<FsUnitLogic>();
            Fill(battleLogic, rets);
            return rets;
        }

    }

    public class TestClass
    {
        public void T(FsBattleLogic battle)
        {
            List<FsUnitLogic> targets = new List<FsUnitLogic>();
            new UnitSelector().InRange(Vector3.zero, 100).Fill(battle, targets);
            foreach (var target in targets)
            {
                //do damage etc..
            }
            //匿名类很多 频繁用这种办法选取new的对象会很多..
            
        }
    }

}