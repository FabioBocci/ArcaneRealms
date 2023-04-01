using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Utils;
using UnityEngine;

namespace ArcaneRealms.Scripts.Effects.OnActivation {

	[CreateAssetMenu(fileName = "New Projectile Damage Effect", menuName = "card effect/Projectile Damage")]
	public class ProjectileWithDamageEffect : OnActivationEffect {

		private static readonly string TARGET_DEFAULT = TargetsEnum.ENEMY_MONSTERS.name;
		private static readonly string DAMAGE_NAME = "PP-Damage-Effect";
		private static readonly string DAMAGE_AMOUNT = "1";


		[SerializeField] GameObject projectile;


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
			throw new System.NotImplementedException();
		}
	}
}