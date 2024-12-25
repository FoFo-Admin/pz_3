using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json;
using System.IO;
using System;

namespace RotaryTheGame.Models
{
    public class GamesHandler
    {
        List<Game> _games;
        public List<Game> Games { get { return _games; } }

        public GamesHandler()
        {
            _games = new List<Game>();
        }

        public Game generateNewGame()
        {
            Game newGame = new Game();

            string code = string.Empty;

            do
            {
                code = newGame.generateCode();
            } while (_games.Any(game => game.Code == code));

            newGame.Code = code;

            _games.Add(newGame);

            return newGame;
        }

        public Game? getGameByCode(string code)
        {
            return _games.FirstOrDefault(game => game.Code == code);
        }

        public List<Dictionary<string, object>>? getPlayerListByGameCode(string code, string? uuid)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return null;
            }

            Player? player = game.getPlayerByUuid(uuid);

            var playerList = game?.getPlayerList();

            return playerList;
        }

        public Player? getPlayerByGameCode(string code, string? uuid)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return null;
            }

            return game.getPlayerByUuid(uuid); 
        }

        public PlayerSelfPrint? getPlayerDeckByGameCode(string code, string? uuid)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return null;
            }

            return game.GetPlayerDeck(uuid);
        }

        public GamePrint? getGameTable(string code, string? uuid)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return null;
            }

            Player? player = game.getPlayerByUuid(uuid);

            if (player == null)
            {
                return null;
            }

            return game.gameTable();
        }

        public VoteResults? getLastVote(string code)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return null;
            }

            if (game.VotesHistory.Count() > 0)
            {
                string deathWinner = game.getWinner(game.DeadVotesHistory.Last());
                string winner = game.getWinner(game.VotesHistory.Last(), deathWinner);
                return new VoteResults(game, game.getPlayerByUuid(winner));
            }
            return null;
        }

        public void setPlayerConnection(string code, string? uuid, string connection)
        {
            Game? game = this.getGameByCode(code);

            if (game != null)
            {
                Player? player = game.getPlayerByUuid(uuid);
                if (player != null)
                {
                    player.ConnectionId = connection;
                }
            }
        }

        public bool isPlayerHost(string code, string? uuid)
        {
            Game? game = this.getGameByCode(code);

            if (game == null)
            {
                return false;
            }

            Player? player = game.getPlayerByUuid(uuid);

            if (player == null)
            {
                return false;
            }

            return player.Host;
        }


        public void Save()
        {
            string json = JsonSerializer.Serialize(this._games);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "save.json");

            File.WriteAllText(filePath, json);
        }

        public void Load()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "save.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    _games = JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading games: {ex.Message}");
                    _games = new List<Game>();
                }
            }
            else
            {
                Console.WriteLine("No save file found. Starting with a new game list.");
                _games = new List<Game>();
            }
        }

    }
}
