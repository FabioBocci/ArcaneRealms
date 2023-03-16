using ArcaneRealms.Scripts.Enums;

namespace ArcaneRealms.Scripts.Utils {
	public static class CardUtils {
		public static string GetName(this Race race) {
			switch(race) {
				case Race.Human:
					return "Human";
				case Race.Beast:
					return "Beast";
				case Race.Human_Beast:
					return "Human-Beast";
				case Race.Goblin:
					return "Goblin";
				case Race.None:
					return "";
				default:
					return "ERROR-Race not implemented!";
			}

		}
	}
}