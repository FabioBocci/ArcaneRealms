using Assets.Scripts.SO;

namespace ArcaneRealms.Scripts.Effects.OnActivation {
	public abstract class OnActivationEffect : CardEffectSO {

		public override bool HasActivationEffect() {
			return true;
		}
	}
}