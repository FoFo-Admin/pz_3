namespace RotaryTheGame.Models
{
    public interface ICard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; }
    }
}
