using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public T Instance { private set; get; }

        protected bool EnsureInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return false;
            }

            Instance = this as T;
            DontDestroyOnLoad(this);
            return true;
        }
        
        protected virtual void Awake()
        {
            EnsureInstance();
        }
    }
}