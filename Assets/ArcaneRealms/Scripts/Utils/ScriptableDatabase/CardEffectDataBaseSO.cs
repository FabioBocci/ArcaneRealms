using System.Collections.Generic;
using ArcaneRealms.Scripts.Cards.Effects.ScriptableEffects;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils.ScriptableDatabase {

	//[CreateAssetMenu(fileName = "Card Effects DB", menuName = "New Effects DB", order = 0)]
	public class CardEffectDataBaseSO : ScriptableObject {
		public List<CardEffectSO> EffectsList = new();
		
	}
}