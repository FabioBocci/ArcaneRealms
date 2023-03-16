using ArcaneRealms.Scripts.Effects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.SO {
	public abstract class CardEffectSO : ScriptableObject {

		[SerializeField] private string Name;

		//TODO : each metod should have a reference (thisCard, thisGame, thisPlayer)

		//Called when the card is posizioned on the field
		public virtual void OnActivation(CardEffect effectWithParam) {

		}

		public virtual bool HasActivationEffect() {
			return false;
		}

		//Called when the card is destroyed from the field
		public virtual void OnDestruction(CardEffect effectWithParam) {

		}

		public virtual bool HasDestuctionEffect() {
			return false;
		}


		//Called when the card is draw from the deck to your hand
		public virtual void OnDraw(CardEffect effectWithParam) {

		}

		public virtual bool HasDrawEffect() {
			return false;
		}

		//Called when the game start
		public virtual void OnGameStart(CardEffect effectWithParam) {

		}

		public virtual bool HasGameStartEffect() {
			return false;
		}


		//Called when ThisCardAttack
		public virtual void OnThisCardAttack(CardEffect effectWithParam) {

		}

		public virtual bool HasThisCardAttackEffect() {
			return false;
		}


		//Called when OtherCardAttack
		public virtual void OnOtherCardAttack(CardEffect effectWithParam) {

		}

		public virtual bool HasOtherCardAttackEffect() {
			return false;
		}



		public abstract string GetDesc(CardEffect effect);

		public abstract List<Parameter> GetDefaultValueDictionary();
	}
}