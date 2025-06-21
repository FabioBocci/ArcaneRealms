using System;
using ArcaneRealms.Scripts.Enums;

namespace ArcaneRealms.Scripts.Interfaces {
	public interface ITargetable {

		public Guid GetTeam();

		public TargetType GetTargetType();

		public Guid GetUnique();
	}
}