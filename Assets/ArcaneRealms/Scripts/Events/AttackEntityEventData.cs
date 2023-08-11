using ArcaneRealms.Scripts.Interfaces;
using ArcaneRealms.Scripts.Players;
using ArcaneRealms.Scripts.Utils.Events;

namespace ArcaneRealms.Scripts.Events
{
    public class AttackEntityEventData : EntityEventData<IDamageable>
    {
        public PlayerInGame Player { get; set; }
        public IDamageable Defender { get; set; }
        public IDamageable Attacker { get => Entity; set => Entity = value; }
        
        public int AttackerAttack { get; set; }
        public int DefenderAttack { get; set; }
        
        public AttackEntityEventData(PlayerInGame player, IDamageable attacker, IDamageable defender, int attack, int defenderAttack) : base(attacker)
        {
            Player = player;
            Defender = defender;
            AttackerAttack = attack;
            DefenderAttack = defenderAttack;
        }
    }
    
    public delegate void AttackEntityEvent(ref AttackEntityEventData entityEventData);
}