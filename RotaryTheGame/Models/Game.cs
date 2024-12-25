using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using RotaryTheGame.Models.CardsModels;
using System.Collections;
using RotaryTheGame.Models.ActionModels;
using System.Numerics;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace RotaryTheGame.Models
{
    public class Game
    {
        List<Player> _players;
        public List<Player> Players { get { return _players; } }

        public string Code { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }

        public GlobalDeck GameDeck { get; }

        public TaskCard Task {  get; set; }
        public List<ForceAction> ForcesHistory {  get; }

        public List<Dictionary<string, List<string>>> VotesHistory { get; }
        public List<Dictionary<string, List<string>>> DeadVotesHistory { get; }

        public int PlayerTurn { get; set; }
        public int Round {  get; set; }

        public Game() 
        {
            _players = new List<Player>();
            GameDeck = new GlobalDeck();
            ForcesHistory = new List<ForceAction>();
            VotesHistory = new List<Dictionary<string, List<string>>>();
            DeadVotesHistory = new List<Dictionary<string, List<string>>>();
            Status = "waiting";
            StatusReason = "waiting";
            PlayerTurn = 0;
            Round = 0;
        }

        public string generateCode()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int charactersLength = characters.Length;
            Random random = new Random();

            string result = string.Empty;

            for (int i = 0; i < 4; i++)
            {
                result += characters[random.Next(charactersLength)];
            }
            return result;
        }

        public Player? getPlayerByUuid(string? uuid)
        {
            return _players.FirstOrDefault(player => player.Uuid == uuid);
        }

        public PlayerSelfPrint? GetPlayerDeck(string? uuid)
        {
            Player? player = getPlayerByUuid(uuid);

            if (player == null)
                return null;

            return new PlayerSelfPrint(player, (player == _players[PlayerTurn] && StatusReason == "round"));
        }

        public Player addPlayer(Player player)
        {
            player.setUuid();
            _players.Add(player);

            return player;
        }

        public List<Dictionary<string, object>> getPlayerList()
        {
            List<Dictionary<string, object>> playerList = new List<Dictionary<string, object>>();
            foreach (Player player in _players)
            {
                playerList.Add(new Dictionary<string, object>
                {
                    {"name", player.Name},
                    {"isHost", player.Host }
                });
            }
            return playerList;
        }

        public void generateGameTheme()
        {
            Random random = new Random();

            int randomIndex = random.Next(GameDeck.TaskCards.Count);
            Task = GameDeck.TaskCards[randomIndex];
            GameDeck.TaskCards.RemoveAt(randomIndex);
        }

        public void generateForce()
        {
            Random random = new Random();

            int randomIndex = random.Next(GameDeck.ForceActions.Count);
            ForcesHistory.Add(GameDeck.ForceActions[randomIndex]);
            GameDeck.ForceActions.RemoveAt(randomIndex);
        }

        public void startGame()
        {
            generateGameTheme();
            generateForce();

            foreach (Player player in _players)
            {
                player.generateDeck(GameDeck);
            }

            Status = "ongoing";
            StatusReason = "round";
        }

        public GamePrint gameTable()
        {
            return new GamePrint(this);
        }

        public int countAvailable()
        {
            int count = 0;
            foreach (Player player in _players)
            {
                if(! (player.isKilled || player.ConnectionId == string.Empty))
                    count++;
            }
            return count;
        }

        public Player? nextMove(Player player, bool isManual = false)
        {
            if(player == _players[PlayerTurn] && StatusReason == "round")
            {
                if (countAvailable() < 2)
                    return null;

                if (!isManual && player.Host && PlayerTurn == 0 && Round == 0)
                    return null;

                if (PlayerTurn + 1 < _players.Count)
                    PlayerTurn++;
                else
                    PlayerTurn = 0;

                if (PlayerTurn == Round % _players.Count)
                {
                    startVoting();
                    return null;
                }

                if(_players[PlayerTurn].isKilled || _players[PlayerTurn].ConnectionId == string.Empty)
                {
                    Player returnPlayer = nextMove(_players[PlayerTurn]);
                    if (returnPlayer != null)
                        return returnPlayer;
                    else
                        return null;
                }

                return _players[PlayerTurn];
            }
            else
            {
                return null;
            }
        }

        public Player? getCurrentMove()
        {
            if (StatusReason == "round")
            {
                if (countAvailable() < 2)
                    return null;

                if (_players[PlayerTurn].isKilled || _players[PlayerTurn].ConnectionId == string.Empty)
                {
                    Player returnPlayer = nextMove(_players[PlayerTurn]);
                    if (returnPlayer != null)
                        return returnPlayer;
                    else
                        return null;
                }

                return _players[PlayerTurn];
            }
            else
            {
                return null;
            }
        }

        public int countVotable(bool killed = false)
        {
            int count = 0;
            foreach (Player player in _players)
            {
                if (player.ConnectionId != String.Empty)
                {
                    if (killed)
                    {
                        if (player.isKilled)
                            count++;
                    }
                    else
                        count++;
                }
            }
            return count;
        }

        public int countVotes(bool killed = false)
        {
            int count = 0;
            foreach (Player player in _players)
            {
                if (player.ConnectionId != String.Empty && player.isVoted)
                {
                    if (killed)
                    {
                        if (player.isKilled)
                            count++;
                    }
                    else
                        count++;
                }
            }
            //foreach(var keyValue in VotesHistory.Last())
            //{
            //    count += keyValue.Value.Count();
            //}
            return count;
        }

        public void startVoting()
        {
            VotesHistory.Add(new Dictionary<string, List<string>>());
            DeadVotesHistory.Add(new Dictionary<string, List<string>>());

            foreach (Player player in _players)
            {
                player.isVoted = false;

                if (!player.isKilled)
                {
                    VotesHistory.Last().Add(player.Uuid, new List<string>());
                    DeadVotesHistory.Last().Add(player.Uuid, new List<string>());
                }
            }

            StatusReason = "voting";
        }

        public string? getWinner(Dictionary<string, List<string>> dict, string? additionalVote = null)
        {
            int maxCount = 0;
            string? winner = null;
            bool isTie = false;

            foreach (var keyValue in dict)
            {
                int currentCount = keyValue.Value.Count;

                if (keyValue.Key == additionalVote)
                    currentCount += 1;

                if (currentCount > maxCount)
                {
                    maxCount = currentCount;
                    winner = keyValue.Key;
                    isTie = false;
                }
                else if (currentCount == maxCount)
                    isTie = true;
            }

            return isTie ? null : winner;
        }

        public VoteResults checkVote()
        {
            VoteResults thisResult;

            if (countVotes() >= countVotable())
            {
                string? deadWinner = getWinner(DeadVotesHistory.Last());
                string? winner = getWinner(VotesHistory.Last(), deadWinner);

                

                if (winner != null) {
                    Player player = getPlayerByUuid(winner);

                    thisResult = new VoteResults(this, player);

                    player.isKilled = true;
                }
                else
                    thisResult = new VoteResults(this);
                nextRound();
            }
            else
                thisResult = new VoteResults(this);

            return thisResult;
        }

        public VoteResults? vote(Player who, int forWhomI)
        {
            Player forWhom = _players[forWhomI];

            if (who.isVoted || StatusReason != "voting")
                return null;

            if (who.isKilled)
                DeadVotesHistory.Last()[forWhom.Uuid].Add(who.Uuid);

            else
                VotesHistory.Last()[forWhom.Uuid].Add(who.Uuid);

            who.isVoted = true;

            return checkVote();
        }

        public void nextRound()
        {
            Round++;
            PlayerTurn = Round % _players.Count;

            generateForce();

            StatusReason = "round";
        }
    }

    public class GamePrint
    {
        public List<PlayerPrint> Players { get; } = new List<PlayerPrint>();
        public string Task { get; set; }
        public string Force { get; set; }
        public int PlayerTurn { get; set; }
        public bool IsVoting { get; set; }

        public GamePrint(Game game) 
        {
            Task = game.Task.Text;
            Force = game.ForcesHistory.Last().Text;
            PlayerTurn = game.PlayerTurn;
            IsVoting = game.StatusReason == "voting";
            foreach (Player player in game.Players)
            {
                Players.Add(new PlayerPrint(player));
            }
        }
    }

    public class VoteInfo
    {
        public string Name { get; set; }
        public List<string> Votes { get; set; }

        public VoteInfo(string name)
        {
            Name = name;
            Votes = new List<string>();
        }
    }


    public class VoteResults
    {
        public Dictionary<int, VoteInfo> Votes { get; set; }
        public int VotesCount { get; set; }
        public int Votable { get; set; }
        public string Result { get; set; }

        public VoteResults(Game game, Player? winner = null)
        {
            Votes = new Dictionary<int, VoteInfo>();

            Votable = game.countVotable();
            VotesCount = game.countVotes();

            foreach (var keyValue in game.VotesHistory.Last())
            {
                string key = keyValue.Key;

                Player player = game.getPlayerByUuid(key);
                int playerI = game.Players.IndexOf(player);

                Votes.Add(playerI,
                          new VoteInfo(player.Name));

                foreach (string value in keyValue.Value)
                {
                    Votes[playerI].Votes.Add(game.getPlayerByUuid(value).Name);
                }
            }

            if(game.countVotes(true) >= game.countVotable(true))
            {
                string? deadWinner = game.getWinner(game.DeadVotesHistory.Last());
                if (deadWinner != null)
                {
                    Player playerDeadWinner = game.getPlayerByUuid(deadWinner);
                    Votes[game.Players.IndexOf(playerDeadWinner)].Votes.Add("_");
                }
            }

            if(winner != null) 
                Result = winner.Name;
            else
                Result = "_";
        }
    }
}
