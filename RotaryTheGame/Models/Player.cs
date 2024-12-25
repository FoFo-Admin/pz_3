using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace RotaryTheGame.Models
{
    public class Player
    {
        [Required(ErrorMessage = "Ви не ввели ім'я")]
        [MinLength(3, ErrorMessage = "Занадто коротке ім'я")]
        [MaxLength(20, ErrorMessage = "Занадто довге ім'я")]
        public string Name { get; set; }

        public string? Uuid { get; set; }
        public string? ConnectionId { get; set; }
        public bool Host { get; set; } = false;

        public bool isKilled { get; set; } = false;
        public bool isVoted { get; set; } = false;

        public PlayerDeck? Deck { get; set; }


        public void setUuid()
        {
            Guid uuid = Guid.NewGuid();
            this.Uuid = uuid.ToString();

            Deck = new PlayerDeck();
        }

        public void setHost()
        {
            this.Host = true;   
        }

        private static T AddRandomCard<T>(List<T> gameDeck)
        {
            Random random = new Random();

            int randomIndex = random.Next(gameDeck.Count);
            T selectedCard = gameDeck[randomIndex];
            gameDeck.RemoveAt(randomIndex);

            return selectedCard;
        }

        public void generateProfession(GlobalDeck gameDeck)
        {
            Deck.ProfessionCard = AddRandomCard(gameDeck.ProfessionCards);
        }

        public void generateSalary(GlobalDeck gameDeck)
        {
            Deck.SalaryCard = AddRandomCard(gameDeck.SalaryCards);
        }

        public void generateCharacteristic(GlobalDeck gameDeck)
        {
            Deck.CharacteristicCard = AddRandomCard(gameDeck.CharacteristicCards);
        }

        public void generateHobbie(GlobalDeck gameDeck)
        {
            Deck.HobbieCard = AddRandomCard(gameDeck.HobbieCards);
        }

        public void generateFact(GlobalDeck gameDeck)
        {
            Deck.FactCards.Add(AddRandomCard(gameDeck.FactCards));
        }

        public void generateItem(GlobalDeck gameDeck)
        {
            Deck.ItemCards.Add(AddRandomCard(gameDeck.ItemCards));
        }

        public void generateAction(GlobalDeck gameDeck)
        {
            Deck.UserActions.Add(AddRandomCard(gameDeck.UserActions));
        }

        public void generateDeck(GlobalDeck gameDeck)
        {
            generateProfession(gameDeck);
            generateSalary(gameDeck);
            generateCharacteristic(gameDeck);
            generateHobbie(gameDeck);

            generateFact(gameDeck);
            generateFact(gameDeck);

            generateItem(gameDeck);
            generateItem(gameDeck);

            generateAction(gameDeck);
            generateAction(gameDeck);
        }

        public void updateDeck(PropertyChangeSchema propertyChange)
        {
            var property = Deck.GetType().GetProperty(propertyChange.Type);

            if (property != null)
            {
                var obj = property.GetValue(Deck);

                if (obj is ICard)
                    ((ICard)obj).IsOpen = propertyChange.Action;
                else if (obj is IAction)
                    ((IAction)obj).IsOpen = propertyChange.Action;
                else if (obj is IEnumerable<ICard>)
                {
                    if (((IEnumerable<ICard>)obj).Count() > propertyChange.I && propertyChange.I >= 0)
                        ((IEnumerable<ICard>)obj).ToList()[propertyChange.I].IsOpen = propertyChange.Action;
                }
                else if (obj is IEnumerable<IAction>)
                {
                    if (((IEnumerable<IAction>)obj).Count() > propertyChange.I)
                        ((IEnumerable<IAction>)obj).ToList()[propertyChange.I].IsOpen = propertyChange.Action;
                }
            }
        }
    }

    public class PlayerPrint
    {
        public string Name { get; set; }

        public PlayerDeckPrint Deck { get; set; }
        public bool IsKilled { get; set; }
        public bool IsVotable {  get; set; }

        public PlayerPrint(Player player)
        {
            Name = player.Name;
            Deck = new PlayerDeckPrint(player.Deck);
            this.IsKilled = player.isKilled || player.ConnectionId == string.Empty;
            IsVotable = !player.isKilled;
        }
    }

    public class PlayerSelfPrint
    {
        public PlayerDeck Deck { get; set; }

        public bool IsTurn { get; set; }
        public bool AbleToVote { get; set; }

        

        public PlayerSelfPrint(Player player, bool IsTurn)
        {
            this.IsTurn = IsTurn;
            this.Deck = player.Deck;
            this.AbleToVote = !player.isVoted;
        }
    }
}
