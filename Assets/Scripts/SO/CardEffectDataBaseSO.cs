using Assets.Scripts.SO;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneRealms.Scripts.SO {

	//[CreateAssetMenu(fileName = "Card Effects DB", menuName = "New Effects DB", order = 0)]
	public class CardEffectDataBaseSO : ScriptableObject {
		public List<CardEffectSO> EffectsList = new();

		public void AddEffect(CardEffectSO effect) {
			EffectsList.Add(effect);
		}

		public int IndexOf(CardEffectSO effect) {
			return EffectsList.IndexOf(effect);
		}

		public CardEffectSO Get(int index) {
			return EffectsList[index];
		}


	}
}