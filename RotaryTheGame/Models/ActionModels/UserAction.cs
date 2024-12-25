namespace RotaryTheGame.Models.ActionModels
{
    public class UserAction : IAction
    {
        public string Text { set; get; }
        public string AffectedAttribute { get; }
        public List<string> Actions { get; }

        public bool IsOpen { set; get; } = false;
    }
}
