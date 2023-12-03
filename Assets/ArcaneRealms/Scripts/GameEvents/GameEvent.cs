
namespace ArcaneRealms.Scripts.GameEvents
{
    public abstract class GameEvent
    {
        public string Label { private set; get; }
        
        public abstract void Undo();
        
        public abstract void Do();

        public string Info()
        {
            return $"{{ {Label} }}";
        }
    }
}