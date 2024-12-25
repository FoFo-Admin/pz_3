using Microsoft.AspNetCore.Identity;
using RotaryTheGame.Models.ActionModels;
using RotaryTheGame.Models.CardsModels;
using System.Diagnostics.Eventing.Reader;

namespace RotaryTheGame.Models
{
    public class PlayerDeck
    {
        public ProfessionCard ProfessionCard { get; set; }
        public SalaryCard SalaryCard { get; set; } 
        public CharacteristicCard CharacteristicCard { get; set; } 
        public HobbieCard HobbieCard { get; set; }
        public List<FactCard> FactCards { get; } = new List<FactCard>();
        public List<ItemCard> ItemCards { get; } = new List<ItemCard>();
        public List<UserAction> UserActions { get; } = new List<UserAction>();
    }

    public class PlayerDeckPrint
    {
        public string ProfessionCard { get; set; }
        public string SalaryCard { get; set; }
        public string CharacteristicCard { get; set; }
        public string HobbieCard { get; set; }
        public List<string> FactCards { get; } = new List<string>();
        public List<string> ItemCards { get; } = new List<string>();
        public List<string> UserActions { get; } = new List<string>();

        public PlayerDeckPrint(PlayerDeck deck)
        {
            ProfessionCard = deck.ProfessionCard.IsOpen ? deck.ProfessionCard.Text : "?";
            SalaryCard = deck.SalaryCard.IsOpen ? deck.SalaryCard.Text : "?";
            CharacteristicCard = deck.CharacteristicCard.IsOpen ? deck.CharacteristicCard.Text : "?";
            HobbieCard = deck.HobbieCard.IsOpen ? deck.HobbieCard.Text : "?";

            foreach(FactCard card in deck.FactCards)
                FactCards.Add(card.IsOpen ? card.Text : "?");
            foreach (ItemCard card in deck.ItemCards)
                ItemCards.Add(card.IsOpen ? card.Text : "?");
            foreach (UserAction card in deck.UserActions)
                UserActions.Add(card.IsOpen ? card.Text : "?");
        }
    }
}
