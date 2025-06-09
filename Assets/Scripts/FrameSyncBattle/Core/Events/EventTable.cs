using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
  public class EventTable
  {
    //Action<T> 是C#默认帮定义的委托对象 如下，delegate实际上是一个方法对象 赋值后相当于执行某个方法
    public delegate void P1Method<in T>(T obj);

    private Dictionary<System.Type, Dictionary<string, Delegate>> eventTable;

    private void RegisterEvent(string name, Delegate handler)
    {
      if (this.eventTable == null)
        this.eventTable = new Dictionary<System.Type, Dictionary<string, Delegate>>();
      Dictionary<string, Delegate> dictionary;
      if (!this.eventTable.TryGetValue(handler.GetType(), out dictionary))
      {
        dictionary = new Dictionary<string, Delegate>();
        this.eventTable.Add(handler.GetType(), dictionary);
      }

      Delegate a;
      if (dictionary.TryGetValue(name, out a))
        dictionary[name] = Delegate.Combine(a, handler); //注意相同key是都保留
      else
        dictionary.Add(name, handler);
    }

    public void RegisterEvent(string name, System.Action handler) => this.RegisterEvent(name, (Delegate)handler);

    public void RegisterEvent<T>(string name, System.Action<T> handler) => this.RegisterEvent(name, (Delegate)handler);

    public void RegisterEvent<T, U>(string name, System.Action<T, U> handler) =>
      this.RegisterEvent(name, (Delegate)handler);

    public void RegisterEvent<T, U, V>(string name, System.Action<T, U, V> handler) =>
      this.RegisterEvent(name, (Delegate)handler);

    private Delegate GetDelegate(string name, System.Type type)
    {
      Dictionary<string, Delegate> dictionary;
      Delegate @delegate;
      return this.eventTable != null && this.eventTable.TryGetValue(type, out dictionary) &&
             dictionary.TryGetValue(name, out @delegate)
        ? @delegate
        : (Delegate)null;
    }

    public void SendEvent(string name)
    {
      if (!(this.GetDelegate(name, typeof(System.Action)) is System.Action action))
        return;
      action();
    }

    public void SendEvent<T>(string name, T arg1)
    {
      if (!(this.GetDelegate(name, typeof(System.Action<T>)) is System.Action<T> action))
        return;
      action(arg1);
    }

    public void SendEvent<T, U>(string name, T arg1, U arg2)
    {
      if (!(this.GetDelegate(name, typeof(System.Action<T, U>)) is System.Action<T, U> action))
        return;
      action(arg1, arg2);
    }

    public void SendEvent<T, U, V>(string name, T arg1, U arg2, V arg3)
    {
      if (!(this.GetDelegate(name, typeof(System.Action<T, U, V>)) is System.Action<T, U, V> action))
        return;
      action(arg1, arg2, arg3);
    }

    private void UnregisterEvent(string name, Delegate handler)
    {
      Dictionary<string, Delegate> dictionary;
      Delegate source;
      if (this.eventTable == null || !this.eventTable.TryGetValue(handler.GetType(), out dictionary) ||
          !dictionary.TryGetValue(name, out source))
        return;
      dictionary[name] = Delegate.Remove(source, handler); //
    }

    public void UnregisterEvent(string name, System.Action handler) => this.UnregisterEvent(name, (Delegate)handler);

    public void UnregisterEvent<T>(string name, System.Action<T> handler) =>
      this.UnregisterEvent(name, (Delegate)handler);

    public void UnregisterEvent<T, U>(string name, System.Action<T, U> handler) =>
      this.UnregisterEvent(name, (Delegate)handler);

    public void UnregisterEvent<T, U, V>(string name, System.Action<T, U, V> handler) =>
      this.UnregisterEvent(name, (Delegate)handler);
  }
}