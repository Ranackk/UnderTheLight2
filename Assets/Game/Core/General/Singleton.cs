using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSDK.Misc
{

    ////////////////////////////////////////////////////////////////

    /// <summary>
    /// Singleton Wrapper of T (Thread safe)
    /// CARE: Does not prevent other instances of T created by constructing T elsewhere
    /// </summary>
    /// <typeparam name="T">Type of the singleton</typeparam>
    public class Singleton<T> where T : class, new() 
    {
        public Singleton() { }

        class SingletonCreator
        {
            static SingletonCreator() { }

            internal static readonly T Instance = new T();
        }

        public static T Instance {
            get { return SingletonCreator.Instance; }
        }
    }

    ////////////////////////////////////////////////////////////////
}

