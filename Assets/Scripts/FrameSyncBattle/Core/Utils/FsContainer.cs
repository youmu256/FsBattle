using System;
using System.Collections.Generic;

namespace FrameSyncBattle
{
    public class FsContainer<T> where T : class
    {
        protected LinkedList<T> List { get; private set; }
        protected LinkedListNode<T> CacheNode { get; private set; }

        public void Add(T t)
        {
            List.AddLast(t);
        }

        public void Remove(T t)
        {
            if (CacheNode != null && CacheNode.Value == t)
            {
                CacheNode = CacheNode.Next;
            }
            List.Remove(t);
        }

        public void RunAll(Action<T> action)
        {
            var current = List.First;
            while (current != null)
            {
                CacheNode = current.Next;
                action.Invoke(current.Value);
                current = CacheNode;
                CacheNode = null;
            }

            while (current!=null)
            {
                action.Invoke(current.Value);
                //如果action中让current直接移除 current整个节点会重置失去next的引用
                current = current.Next;
            }
        }
    }
}