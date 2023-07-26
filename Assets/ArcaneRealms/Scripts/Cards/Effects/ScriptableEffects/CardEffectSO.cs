using ArcaneRealms.Scripts.Cards;
using ArcaneRealms.Scripts.Effects;
using ArcaneRealms.Scripts.Enums;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.SO;
using UnityEngine;

namespace Assets.Scripts.SO {
	public abstract class CardEffectSO : ScriptableObject {
		public static readonly string TARGET_PARAM_NAME = "target";
		protected static readonly string TARGET_TYPE = typeof(TargetsEnum).AssemblyQualifiedName;
		protected static readonly string INT = typeof(int).AssemblyQualifiedName;
		protected static readonly string BOOL = typeof(bool).AssemblyQualifiedName;
		protected static readonly string FLOAT = typeof(float).AssemblyQualifiedName;
		protected static readonly string STRING = typeof(string).AssemblyQualifiedName;

		protected static CardEffectDataBaseSO DatabaseStatic = null;

		[SerializeField] private string Name;


		[SerializeField] private CardEffectDataBaseSO database;

		private void OnValidate() {
			if(database != null && DatabaseStatic == null) {
				DatabaseStatic = database;
			}

			if(database == null && DatabaseStatic != null) {
				database = DatabaseStatic;
			}
			if(DatabaseStatic != null) {
				if(!database.EffectsList.Contains(this)) {
					database.EffectsList.Add(this);
				}
			}
		}

		//TODO : each metod should have a reference (thisCard, thisGame, thisPlayer)

		//Called when the card is posizioned on the field
		public virtual void OnActivation(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasActivationEffect() {
			return false;
		}

		//Called when the card is destroyed from the field
		public virtual void OnDestruction(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasDestructionEffect() {
			return false;
		}


		//Called when the card is draw from the deck to your hand
		public virtual void OnDraw(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasDrawEffect() {
			return false;
		}

		//Called when the game start
		public virtual void OnGameStart(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasGameStartEffect() {
			return false;
		}


		//Called when ThisCardAttack
		public virtual void OnThisCardAttack(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasThisCardAttackEffect() {
			return false;
		}


		//Called when OtherCardAttack
		public virtual void OnOtherCardAttack(PlayerInGame owner, CardInGame card, EffectParameters parameters) {

		}

		public virtual bool HasOtherCardAttackEffect() {
			return false;
		}


		//if this effect require a target
		public virtual bool RequireTargetToRun(EffectParameters parameters) {
			return false;
		}


		public abstract string GetDesc(EffectParameters parameters);

		public abstract EffectParameters GetDefaultValueDictionary();
	}
}