namespace RotaryTheGame.Models.CardsModels
{
    public class HobbieCard : ICard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; } = false;
    }
}
