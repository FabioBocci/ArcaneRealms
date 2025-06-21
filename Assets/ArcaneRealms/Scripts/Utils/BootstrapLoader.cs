using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneRealms.Scripts.Utils
{
    public static class BootstrapLoader
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BootstrapSceneCheck()
        {
            // Store the wanted scene index
            var index = SceneManager.GetActiveScene().buildIndex;

            if (index != 0)
            {
                // Load bootstrap scene
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
            
        }
#endif
    }
}