using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace JSDK.Misc
{
    public class ObjectPool<T> where T : new() {
        private readonly Queue<T>   m_Items;
        private bool                m_AssertPoolGrowth;

        public ObjectPool(int startSize, bool assertPoolGrowth = false)
        {
            m_Items             = new Queue<T>(startSize);
            m_AssertPoolGrowth  = assertPoolGrowth;

            for (int i = 0; i < startSize; i++)
            {
                m_Items.Enqueue(new T());
            }
        }

        ~ObjectPool()
        {
            // Do we need to cleanup something?
        }

        public T Get()
        {
            if (m_Items.Count != 0)
            {
                return m_Items.Dequeue();
            }

            Debug.Assert(!m_AssertPoolGrowth, "Object Pool Growing");
            return new T();
        }

        public void Release(T item)
        {
            m_Items.Enqueue(item);
        }
    }
}
