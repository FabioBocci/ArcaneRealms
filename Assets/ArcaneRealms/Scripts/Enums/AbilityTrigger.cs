namespace ArcaneRealms.Scripts.Enums
{
    public enum AbilityTrigger
    {
        None = 0,

        Ongoing = 2, //Always active (does not work with all effects)
        Activate = 5, //Action

        OnPlay = 10, //When playeds
        OnPlayOther = 12, //When another card played

        StartOfTurn = 20, //Every turn
        EndOfTurn = 22, //Every turn

        OnBeforeAttack = 30, //When attacking, before damage
        OnAfterAttack = 31, //When attacking, after damage if still alive
        OnBeforeDefend = 32, //When being attacked, before damage
        OnAfterDefend = 33, //When being attacked, after damage if still alive
        OnKill = 35, //When killing another card during an attack

        OnDeath = 40, //When dying
        OnDeathOther = 42 //When another dying
    }
}
