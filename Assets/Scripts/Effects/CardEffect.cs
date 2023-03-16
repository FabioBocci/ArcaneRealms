using Assets.Scripts.SO;
using System;
using System.Collections.Generic;

namespace ArcaneRealms.Scripts.Effects {

	[Serializable]
	public class CardEffect {

		public CardEffectSO effectSO;
		public List<Parameter> parameters = new();


		//Called when the card is posizioned on the field
		public virtual void OnActivation() => effectSO.OnActivation(this);

		public virtual bool HasActivationEffect() => effectSO.HasActivationEffect();


		//Called when the card is destroyed from the field
		public virtual void OnDestruction() => effectSO.OnDestruction(this);

		public virtual bool HasDestuctionEffect() => effectSO.HasDestuctionEffect();


		//Called when the card is draw from the deck to your hand
		public virtual void OnDraw() => effectSO.OnDraw(this);

		public virtual bool HasDrawEffect() => effectSO.HasDrawEffect();



		//Called when the game start
		public virtual void OnGameStart() => effectSO.OnGameStart(this);

		public virtual bool HasGameStartEffect() => effectSO.HasGameStartEffect();


		//Called when ThisCardAttack
		public virtual void OnThisCardAttack() => effectSO.OnThisCardAttack(this);

		public virtual bool HasThisCardAttackEffect() => effectSO.HasThisCardAttackEffect();


		//Called when OtherCardAttack
		public virtual void OnOtherCardAttack() => effectSO.OnOtherCardAttack(this);

		public virtual bool HasOtherCardAttackEffect() => effectSO.HasOtherCardAttackEffect();



		public string GetDesc() {
			return effectSO.GetDesc(this);
		}

		public string GetParameter(string key) {
			var parameter = parameters.Find(p => p.Key == key);
			if(parameter == null) {
				return null;
			} else {
				return parameter.Value;
			}
		}

	}



	public class Parameter {
		public string Key;
		public string Value;
		public string Type;
	}

}