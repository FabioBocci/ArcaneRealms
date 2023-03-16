using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneRealms.Scripts.SO {
	//[CreateAssetMenu(fileName = "new Database", menuName = "Cards/Create Database", order = 4)]
	public class CardInfoDataBase : ScriptableObject {
		public List<CardInfoSO> Cards = new();

#if UNITY_EDITOR

		[ContextMenu("Load")]
		public void Load() {

			Cards.Sort((c1, c2) => c1.ID.CompareTo(c2.ID));

			List<CardInfoSO> noIDCards = new();

			int nextID = 1;
			int currentID = 0;
			foreach(CardInfoSO cardC in Cards) {
				if(char.IsLetter(cardC.ID.FirstOrDefault())) {
					if(cardC.ID.FirstOrDefault() == 'I') {
						noIDCards.Add(cardC);
					}
				} else {
					if(int.TryParse(cardC.ID, out currentID)) {
						nextID = Mathf.Max(nextID, currentID + 1);
					}
				}
			}

			foreach(CardInfoSO card in noIDCards) {
				card.ID = nextID.ToString();

				EditorUtility.SetDirty(card);
				nextID++;
			}

			Cards.Sort((c1, c2) => c1.ID.CompareTo(c2.ID));

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

		}
#endif

		public CardInfoSO GetCardFromID(string id) {
			return Cards.Find(x => x.ID == id);
		}

	}
}