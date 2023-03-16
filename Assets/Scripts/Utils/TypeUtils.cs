using ArcaneRealms.Scripts.Effects;
using System;

namespace ArcaneRealms.Scripts.Utils {
	public static class TypeUtils {
		public static T GetValueOrDefault<T>(this CardEffect info, string key, T defaultValue) {
			string value = info.GetParameter(key);

			switch(defaultValue) {
				case int:
					int newValue;
					if(int.TryParse(value, out newValue)) {
						return (T) Convert.ChangeType(newValue, typeof(T));
					} else {
						return defaultValue;
					}
				case float:
					float newFloatValue;
					if(float.TryParse(value, out newFloatValue)) {
						return (T) Convert.ChangeType(newFloatValue, typeof(T));
					} else {
						return defaultValue;
					}
				case string:
					if(value == null || value.Length == 0) {
						return (T) Convert.ChangeType(value, typeof(T));
					}
					break;
				case bool:
					bool newBoolValue;
					if(bool.TryParse(value, out newBoolValue)) {
						return (T) Convert.ChangeType(newBoolValue, typeof(T));
					} else {
						return defaultValue;
					}
				/*case CardEffectTarget:
					CardEffectTarget newTargetValue;
					if(Enum.TryParse(value, true, out newTargetValue)) {
						return (T) Convert.ChangeType(newTargetValue, typeof(T));
					} else {
						return defaultValue;
					}*/
				// add more cases for other types as needed
				default:
					throw new ArgumentException($"Type {typeof(T)} not supported");
			}

			throw new ArgumentException($"Type {typeof(T)} not supported");
		}
	}
}