using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcaneRealms.Scripts.Cards.Effects.ScriptableEffects;
using ArcaneRealms.Scripts.Cards.GameCards;
using ArcaneRealms.Scripts.Players;
using NUnit.Framework;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.Effects {

	[Serializable]
	public class CardEffect {

		public CardEffectSO effectSO;
		[SerializeField] public EffectParameters effectParameters = new();
		public EffectTargets effectTargets = new();


		//Called when the card is posizioned on the field
		public virtual async Task OnActivation(PlayerInGame owner, CardInGame card) => await effectSO.OnActivation(owner, card, effectParameters, effectTargets);

		public virtual bool HasActivationEffect() => effectSO.HasActivationEffect();


		//Called when the card is destroyed from the field
		public virtual async Task OnDestruction(PlayerInGame owner, CardInGame card) => await effectSO.OnDestruction(owner, card, effectParameters, effectTargets);

		public virtual bool HasDestructionEffect() => effectSO.HasDestructionEffect();


		//Called when the card is draw from the deck to your hand
		public virtual async Task OnDraw(PlayerInGame owner, CardInGame card) => await effectSO.OnDraw(owner, card, effectParameters, effectTargets);

		public virtual bool HasDrawEffect(PlayerInGame owner, CardInGame card) => effectSO.HasDrawEffect();



		//Called when the game start
		public virtual async Task OnGameStart(PlayerInGame owner, CardInGame card) => await effectSO.OnGameStart(owner, card, effectParameters, effectTargets);

		public virtual bool HasGameStartEffect() => effectSO.HasGameStartEffect();


		//Called when ThisCardAttack
		public virtual async Task OnThisCardAttack(PlayerInGame owner, CardInGame card) => await effectSO.OnThisCardAttack(owner, card, effectParameters, effectTargets);

		public virtual bool HasThisCardAttackEffect() => effectSO.HasThisCardAttackEffect();


		//Called when OtherCardAttack
		public virtual async Task OnOtherCardAttack(PlayerInGame owner, CardInGame card) => await effectSO.OnOtherCardAttack(owner, card, effectParameters, effectTargets);

		public virtual bool HasOtherCardAttackEffect() => effectSO.HasOtherCardAttackEffect();

		public virtual bool RequireTargetToRun() => effectSO.RequireTargetToRun(effectParameters);
		
		public string GetDesc() => effectSO.GetDesc(effectParameters);


	}

	[Serializable]
	public class EffectTargets
	{
		public List<Guid> targets = new();

	}
}