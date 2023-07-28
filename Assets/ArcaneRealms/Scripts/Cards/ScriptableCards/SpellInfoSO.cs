using ArcaneRealms.Scripts.Cards.ScriptableCards;
using ArcaneRealms.Scripts.Enums;
using UnityEngine;

namespace ArcaneRealms.Scripts.SO {

	[CreateAssetMenu(fileName = "New Spell", menuName = "Cards/New Spell", order = 1)]
	public class SpellInfoSO : CardInfoSO {
		public SpellType SpellType;

	}
}