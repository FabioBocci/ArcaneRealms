using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.ScriptableCards {

	[CreateAssetMenu(fileName = "New Spell", menuName = "Cards/New Spell", order = 1)]
	public class SpellInfoSO : CardInfoSO {
		public SpellType SpellType;

	}
}