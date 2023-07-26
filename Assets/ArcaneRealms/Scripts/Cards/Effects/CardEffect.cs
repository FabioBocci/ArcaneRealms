using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Players;
using Assets.Scripts.SO;
using System;
using UnityEngine;

namespace ArcaneRealms.Scripts.Effects {

	[Serializable]
	public class CardEffect {

		public CardEffectSO effectSO;
		[SerializeField] public EffectParameters effectParameters = new();


		//Called when the card is posizioned on the field
		public virtual void OnActivation(PlayerInGame owner, CardInGame card) => effectSO.OnActivation(owner, card, effectParameters);

		public virtual bool HasActivationEffect() => effectSO.HasActivationEffect();


		//Called when the card is destroyed from the field
		public virtual void OnDestruction(PlayerInGame owner, CardInGame card) => effectSO.OnDestruction(owner, card, effectParameters);

		public virtual bool HasDestructionEffect() => effectSO.HasDestructionEffect();


		//Called when the card is draw from the deck to your hand
		public virtual void OnDraw(PlayerInGame owner, CardInGame card) => effectSO.OnDraw(owner, card, effectParameters);

		public virtual bool HasDrawEffect(PlayerInGame owner, CardInGame card) => effectSO.HasDrawEffect();



		//Called when the game start
		public virtual void OnGameStart(PlayerInGame owner, CardInGame card) => effectSO.OnGameStart(owner, card, effectParameters);

		public virtual bool HasGameStartEffect() => effectSO.HasGameStartEffect();


		//Called when ThisCardAttack
		public virtual void OnThisCardAttack(PlayerInGame owner, CardInGame card) => effectSO.OnThisCardAttack(owner, card, effectParameters);

		public virtual bool HasThisCardAttackEffect() => effectSO.HasThisCardAttackEffect();


		//Called when OtherCardAttack
		public virtual void OnOtherCardAttack(PlayerInGame owner, CardInGame card) => effectSO.OnOtherCardAttack(owner, card, effectParameters);

		public virtual bool HasOtherCardAttackEffect() => effectSO.HasOtherCardAttackEffect();

		public virtual bool RequireTargetToRun() => effectSO.RequireTargetToRun(effectParameters);

		public string GetDesc() => effectSO.GetDesc(effectParameters);


	}

}