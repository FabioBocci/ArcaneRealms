using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { private set; get; }

        public bool dontDestroy = true;

        protected bool EnsureInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return false;
            }

            Instance = this as T;
            if (dontDestroy)
            {
                DontDestroyOnLoad(this);
            }
            return true;
        }
        
        protected virtual void Awake()
        {
            EnsureInstance();
        }
    }
}