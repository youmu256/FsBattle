using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FrameSyncBattle
{
    public class FsLinkedList<T> :ICollection<T> where T : class
    {
        protected LinkedList<T> List { get; private set; } = new();
        protected LinkedListNode<T> CacheNode { get; private set; }
        public LinkedListNode<T> First => List.First;
        public LinkedListNode<T> Last => List.Last;


        public void Add(T t)
        {
            List.AddLast(t);
        }

        public void Clear()
        {
            List.Clear();
        }

        public bool Contains(T item)
        {
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            List.CopyTo(array,arrayIndex);
        }

        public bool Remove(T t)
        {
            if (CacheNode != null && CacheNode.Value == t)
            {
                CacheNode = CacheNode.Next;
            }
            return List.Remove(t);
        }

        public int Count => List.Count;
        public bool IsReadOnly => false;
        
        /// <summary>
        /// 注意避免闭包引用问题
        /// 遍历中如果有新增元素也会遍历到 前元素移除也不会断开
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<T> action)
        {
            var current = List.First;
            while (current != null)
            {
                CacheNode = current.Next;
                action.Invoke(current.Value);
                current = CacheNode;
                CacheNode = null;
            }
            /*
            while (current!=null)
            {
                action.Invoke(current.Value);
                //如果action中让current直接移除 current整个节点会重置失去next的引用
                current = current.Next;
            }
            */
        }

        /// <summary>
        /// 有闭包问题应该用这个方式
        /// 泛型+元组的方式可以避免匿名类闭包导致的GC毛病
        /// 遍历中如果有新增元素也会遍历到 前元素移除也不会断开
        /// </summary>
        /// <param name="param"></param>
        /// <param name="action"></param>
        /// <typeparam name="V"></typeparam>
        public void ForEach<V>(ref V param,Action<T, V> action)
        {
            //ref 防止结构体传参时导致的引用复制带来的性能问题
            var current = List.First;
            while (current != null)
            {
                CacheNode = current.Next;
                action.Invoke(current.Value, param);
                current = CacheNode;
                CacheNode = null;
            }
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }
    }
}