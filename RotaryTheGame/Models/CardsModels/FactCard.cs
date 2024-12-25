namespace RotaryTheGame.Models.CardsModels
{
    public class FactCard : ICard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; } = false;
    }
}
