namespace RotaryTheGame.Models
{
    public interface IAction
    {
        public string Text { set; get; }
        public string AffectedAttribute { get; }
        public List<string> Actions { get; }

        public bool IsOpen { set; get; } 
    }
}
