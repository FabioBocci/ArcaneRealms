using System.Collections.Generic;
using System.Threading.Tasks;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Managers;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Systems;
using ArcaneRealms.Scripts.Utils;
using ArcaneRealms.Scripts.Utils.EffectsUtils;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.Effects.OnActivation {

	[CreateAssetMenu(fileName = "New Projectile Damage Effect", menuName = "card effect/Projectile Damage")]
	public class ProjectileWithDamageEffect : OnActivationEffect {

		private static readonly string TARGET_DEFAULT = TargetsEnum.ENEMY_MONSTERS.name;
		private static readonly string DAMAGE_NAME = "PP-Damage-Effect";
		private static readonly string DAMAGE_AMOUNT = "1";


		[SerializeField] GameObject projectile;


		public override async Task OnActivation(PlayerInGame owner, CardInGame card, EffectParameters parameters, EffectTargets targets)
		{
			if (NetworkManagerHelper.Instance.IsClient)
			{
				List<Task> tasks = new();
				foreach (var target in targets.targets)
				{
					MonsterPlatformController platformController = FieldManager.Instance.GetPlatformController(target);
					tasks.Add(EffectsUtils.LaunchProjectile(projectile, platformController));
				}

				await Task.WhenAll(tasks);
				//TODO - damage?
				Debug.Log("DAMAGE FROM SPELL GGGG");
				
			} else if (NetworkManagerHelper.Instance.IsServer)
			{

			}
			
			
		}

		public override bool RequireTargetToRun(EffectParameters parameters) {
			TargetsEnum target = parameters.GetValueOrDefault(TARGET_PARAM_NAME, TargetsEnum.ENEMY_MONSTERS);

			return target != TargetsEnum.RANDOM_MONSTERS || target != TargetsEnum.RANDOM_PLAYERS;
		}

		public override EffectParameters GetDefaultValueDictionary() {
			return new EffectParameters(new Parameter[] {
				new Parameter() { Key = DAMAGE_NAME,Value = DAMAGE_AMOUNT, Type = typeof(int).AssemblyQualifiedName},
				new Parameter() { Key = TARGET_PARAM_NAME, Value = TARGET_DEFAULT, Type = TARGET_TYPE },
			});
		}

		public override string GetDesc(EffectParameters parameters) {
			return "";
		}
	}
}