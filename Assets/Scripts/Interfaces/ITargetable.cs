using ArcaneRealms.Scripts.Enums;

namespace ArcaneRealms.Scripts.Interfaces {
	public interface ITargetable {

		public ulong GetTeam();

		public TargetType GetTargetType();
	}
}