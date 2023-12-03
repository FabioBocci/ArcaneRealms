using System.Threading.Tasks;
using ArcaneRealms.Scripts.Cards.GameCards;
using Unity.VisualScripting;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils.EffectsUtils
{
    public static class EffectsUtils
    {
        //TODO - add other options
        //TODO - add spawn position
        public static async Task LaunchProjectile(GameObject projectilePrefab, MonsterPlatformController target)
        {
            GameObject newGo = Object.Instantiate(projectilePrefab);
            ProjectileBehaviour pb = newGo.GetOrAddComponent<ProjectileBehaviour>();
            pb.SetTarget(target);
            await pb.WaitForHit();
        }
    }
}