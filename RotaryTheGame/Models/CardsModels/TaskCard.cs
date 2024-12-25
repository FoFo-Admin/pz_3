namespace RotaryTheGame.Models.CardsModels
{
    public class TaskCard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; } = true;
    }
}
