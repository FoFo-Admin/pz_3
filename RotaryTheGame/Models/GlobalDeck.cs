using RotaryTheGame.Models.ActionModels;
using RotaryTheGame.Models.CardsModels;
using System.Text.Json;

namespace RotaryTheGame.Models
{
    public class GlobalDeck
    {
        public List<ProfessionCard> ProfessionCards { get; }
        public List<SalaryCard> SalaryCards { get; }
        public List<CharacteristicCard> CharacteristicCards { get; }
        public List<HobbieCard> HobbieCards { get; } 
        public List<FactCard> FactCards { get; } 
        public List<ItemCard> ItemCards { get; }
        public List<UserAction> UserActions { get; }


        public List<ForceAction> ForceActions { get; }
        public List<TaskCard> TaskCards { get; }


        public GlobalDeck() 
        {
            ProfessionCards = JsonSerializer.Deserialize<List<ProfessionCard>>(File.ReadAllText("./wwwroot/cards/profession.json")) ?? throw new Exception();
            SalaryCards = JsonSerializer.Deserialize<List<SalaryCard>>(File.ReadAllText("./wwwroot/cards/salary.json")) ?? throw new Exception();
            CharacteristicCards = JsonSerializer.Deserialize<List<CharacteristicCard>>(File.ReadAllText("./wwwroot/cards/char.json")) ?? throw new Exception();
            HobbieCards = JsonSerializer.Deserialize<List<HobbieCard>>(File.ReadAllText("./wwwroot/cards/hobbies.json")) ?? throw new Exception();
            FactCards = JsonSerializer.Deserialize<List<FactCard>>(File.ReadAllText("./wwwroot/cards/facts.json")) ?? throw new Exception();
            ItemCards = JsonSerializer.Deserialize<List<ItemCard>>(File.ReadAllText("./wwwroot/cards/items.json")) ?? throw new Exception();
            UserActions = JsonSerializer.Deserialize<List<UserAction>>(File.ReadAllText("./wwwroot/cards/dia.json")) ?? throw new Exception();
            ForceActions = JsonSerializer.Deserialize<List<ForceAction>>(File.ReadAllText("./wwwroot/cards/forces.json")) ?? throw new Exception();
            TaskCards = JsonSerializer.Deserialize<List<TaskCard>>(File.ReadAllText("./wwwroot/cards/themes.json")) ?? throw new Exception();
        }
    }
}
