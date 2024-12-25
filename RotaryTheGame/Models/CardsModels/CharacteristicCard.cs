namespace RotaryTheGame.Models.CardsModels
{
    public class CharacteristicCard : ICard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; } = false;
    }
}
