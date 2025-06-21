namespace ArcaneRealms.Scripts.Interfaces
{
    public interface IDamageable : ITargetable
    {
        public int GetHealth();

        public int GetMaxHealth();

        public void Damage(int damage);
        
        public void Heal(int amount);

        public bool IsAlive() => GetHealth() >= 0;
    }
}