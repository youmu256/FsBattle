using System.Collections;
using System.Collections.Generic;
using FrameSyncBattle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Tester
{
    // A Test behaves as an ordinary method
    [Test]
    public void TesterProperty()
    {
        FsUnitProperty property = new FsUnitProperty(null);
        property.SetPropertyBase(FsUnitPropertyType.HpMax,1000);
        property.Modify(FsUnitPropertyType.HpMax,NumericOperation.Add,FsPropertyLevel.Lv1,1000);
        property.Modify(FsUnitPropertyType.HpMax,NumericOperation.Pct,FsPropertyLevel.Lv1,100);
        Assert.IsTrue(property.Get(FsUnitPropertyType.HpMax)== 3000);
        
        //3000 - 5000 = -2000 但是Get返回的是限制后的属性
        property.Modify(FsUnitPropertyType.HpMax,NumericOperation.Add,FsPropertyLevel.Lv2,-5000);
        Debug.Log($"Value:{property.Get(FsUnitPropertyType.HpMax)},RawValue:{property.GetRaw(FsUnitPropertyType.HpMax)}");
        Assert.IsTrue(property.Get(FsUnitPropertyType.HpMax)== 0);
        Assert.IsTrue(property.GetRaw(FsUnitPropertyType.HpMax)== -2000);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TesterWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
