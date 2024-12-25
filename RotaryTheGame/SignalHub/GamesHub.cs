using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using RotaryTheGame.Models;
using System;

namespace RotaryTheGame.SignalHub
{
    public class GamesHub : Hub
    {
        private readonly GamesHandler _gamesHandler;

        public GamesHub(GamesHandler gamesHandler)
        {
            _gamesHandler = gamesHandler;
        }

        public override async Task OnConnectedAsync()
        {
            HttpContext? context = Context.GetHttpContext();
            if (context != null)
            {
                string code = context.Request.Query["code"].ToString();

                Player? player = _gamesHandler.getPlayerByGameCode(code, context.Request.Cookies["uuid"]);

                if (player != null)
                {
                    player.ConnectionId = Context.ConnectionId;

                    Game? game = _gamesHandler.getGameByCode(code);
                    if (game.Status == "ongoing")
                    {
                        GamePrint? gamePrint = _gamesHandler.getGameTable(code, context.Request.Cookies["uuid"]);

                        foreach (Player p in game.Players)
                        {
                            this.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
                        }

                        if(game.StatusReason == "voting")
                        {
                            VoteResults? voteResults = game.checkVote();

                            if (voteResults != null)
                            {
                                foreach (Player p in game.Players)
                                {
                                    PlayerSelfPrint? eachDeck = _gamesHandler.getPlayerDeckByGameCode(code, p.Uuid);

                                    this.Clients.Clients(p.ConnectionId).SendAsync("UpdateDeck", eachDeck);
                                    this.Clients.Clients(p.ConnectionId).SendAsync("UpdateVote", voteResults);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Context.Abort();
                }

            }
            else
            {
                Context.Abort();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            HttpContext? context = Context.GetHttpContext();
            if (context != null)
            {
                string code = context.Request.Query["code"].ToString();

                Player? player = _gamesHandler.getPlayerByGameCode(code, context.Request.Cookies["uuid"]);

                if (player != null)
                {
                    player.ConnectionId = String.Empty;

                    Game? game = _gamesHandler.getGameByCode(code);
                    if (game.Status == "ongoing")
                    {
                        Player? nextPlayer = game.nextMove(player);

                        if (nextPlayer != null)
                        {
                            PlayerSelfPrint? playerNextDeck = _gamesHandler.getPlayerDeckByGameCode(code, nextPlayer.Uuid);
                            this.Clients.Clients(nextPlayer.ConnectionId).SendAsync("UpdateDeck", playerNextDeck);
                        }

                        GamePrint? gamePrint = _gamesHandler.getGameTable(code, context.Request.Cookies["uuid"]);

                        foreach (Player p in game.Players)
                        {
                            this.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
                        }

                        if (game.StatusReason == "voting")
                        {
                            VoteResults? voteResults = game.checkVote();

                            if (voteResults != null)
                            {
                                foreach (Player p in game.Players)
                                {
                                    this.Clients.Clients(p.ConnectionId).SendAsync("UpdateVote", voteResults);
                                }
                            }

                            if (game.StatusReason == "round")
                            {
                                Player? currentMovePlayer = game.getCurrentMove();

                                if (currentMovePlayer != null)
                                {
                                    PlayerSelfPrint? currentMovePlayerDeck = _gamesHandler.getPlayerDeckByGameCode(code, currentMovePlayer.Uuid);
                                    this.Clients.Clients(currentMovePlayer.ConnectionId).SendAsync("UpdateDeck", currentMovePlayerDeck);

                                    gamePrint = _gamesHandler.getGameTable(code, context.Request.Cookies["uuid"]);
                                    foreach (Player p in game.Players)
                                    {
                                        PlayerSelfPrint? eachDeck = _gamesHandler.getPlayerDeckByGameCode(code, p.Uuid);

                                        this.Clients.Clients(p.ConnectionId).SendAsync("UpdateDeck", eachDeck);
                                        this.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task sendPlayerJoinedMessage(string code)
        {
            Game? game = _gamesHandler.getGameByCode(code);
            if (game != null)
            {
                List<Dictionary<string, object>> playerList = game.getPlayerList();

                foreach (Player player in game.Players)
                {
                    await Clients.Clients(player.ConnectionId).SendAsync("PlayerJoined", playerList);
                }
            }
        }
    }
}
