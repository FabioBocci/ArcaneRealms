using ArcaneRealms.Scripts.Cards.Effects.ScriptableEffects;

namespace ArcaneRealms.Scripts.Cards.Effects.OnActivation {
	public abstract class OnActivationEffect : CardEffectSO {

		public override bool HasActivationEffect() {
			return true;
		}
	}
}