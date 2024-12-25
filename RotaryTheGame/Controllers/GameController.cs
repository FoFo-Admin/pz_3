using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RotaryTheGame.Models;
using RotaryTheGame.SignalHub;

namespace RotaryTheGame.Controllers
{
    public class GameController : Controller
    {
        private readonly GamesHandler _gamesHandler;
        private readonly IHubContext<GamesHub> _hubContext;

        public GameController(GamesHandler gamesHandler, IHubContext<GamesHub> hubContext)
        {
            _gamesHandler = gamesHandler;
            _hubContext = hubContext;
        }

        public IActionResult Play(string id)
        {
            GamePrint? gamePrint = _gamesHandler.getGameTable(id, HttpContext.Request.Cookies["uuid"]);
            PlayerSelfPrint? playerDeck = _gamesHandler.getPlayerDeckByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            if (gamePrint == null && playerDeck == null)
            {
                return RedirectToAction("Index", "Home");
            }

            VoteResults? voteResults = _gamesHandler.getLastVote(id);

            ViewData["table"] = gamePrint;
            ViewData["deck"] = playerDeck;
            ViewData["vote"] = voteResults;

            ViewData["code"] = id;
            //return Json(gamePrint);
            return View();
        }

        [HttpPost]
        public IActionResult PropertyChange(string id, [FromBody] PropertyChangeSchema body)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game == null)
            {
                return Json(false);
            }

            Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

            if (player == null)
            {
                return Json(false);
            }

            player.updateDeck(body);

            GamePrint? gamePrint = _gamesHandler.getGameTable(id, HttpContext.Request.Cookies["uuid"]);
            PlayerSelfPrint? playerDeck = _gamesHandler.getPlayerDeckByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            _hubContext.Clients.Clients(player.ConnectionId).SendAsync("UpdateDeck", playerDeck);

            foreach (Player p in game.Players)
            {
                _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
            }

            return Json(true);
        }

        [HttpPost]
        public IActionResult NextMove(string id)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game == null)
            {
                return Json(false);
            }

            Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

            if (player == null)
            {
                return Json(false);
            }

            Player? nextPlayer = game.nextMove(player, true);

            if (nextPlayer != null)
            {
                PlayerSelfPrint? playerNextDeck = _gamesHandler.getPlayerDeckByGameCode(id, nextPlayer.Uuid);
                _hubContext.Clients.Clients(nextPlayer.ConnectionId).SendAsync("UpdateDeck", playerNextDeck);
            }

            GamePrint? gamePrint = _gamesHandler.getGameTable(id, HttpContext.Request.Cookies["uuid"]);
            PlayerSelfPrint? playerDeck = _gamesHandler.getPlayerDeckByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            _hubContext.Clients.Clients(player.ConnectionId).SendAsync("UpdateDeck", playerDeck);

            foreach (Player p in game.Players)
            {
                _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
            }

            if (game.StatusReason == "voting")
            {
                VoteResults voteResults = game.checkVote();
                foreach (Player p in game.Players)
                {
                    PlayerSelfPrint? eachDeck = _gamesHandler.getPlayerDeckByGameCode(id, p.Uuid);

                    _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateDeck", eachDeck);
                    _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
                    _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateVote", voteResults);
                }
            }

            return Json(true);
        }


        [HttpPost]
        public IActionResult Vote(string id, int i)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game == null)
            {
                return Json(false);
            }

            Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

            if (player == null)
            {
                return Json(false);
            }

            VoteResults? voteResults = game.vote(player, i);

            if (voteResults != null)
            {
                foreach (Player p in game.Players)
                {
                    _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateVote", voteResults);
                }
            }

            GamePrint? gamePrint = _gamesHandler.getGameTable(id, HttpContext.Request.Cookies["uuid"]);
            PlayerSelfPrint? playerDeck = _gamesHandler.getPlayerDeckByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            _hubContext.Clients.Clients(player.ConnectionId).SendAsync("UpdateDeck", playerDeck);

            foreach (Player p in game.Players)
            {
                _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
            }

            if (game.StatusReason == "round")
            {
                Player? currentMovePlayer = game.getCurrentMove();

                if (currentMovePlayer != null)
                {
                    PlayerSelfPrint? currentMovePlayerDeck = _gamesHandler.getPlayerDeckByGameCode(id, currentMovePlayer.Uuid);
                    _hubContext.Clients.Clients(currentMovePlayer.ConnectionId).SendAsync("UpdateDeck", currentMovePlayerDeck);

                    gamePrint = _gamesHandler.getGameTable(id, HttpContext.Request.Cookies["uuid"]);
                    foreach (Player p in game.Players)
                    {
                        PlayerSelfPrint? eachDeck = _gamesHandler.getPlayerDeckByGameCode(id, p.Uuid);

                        _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateDeck", eachDeck);
                        _hubContext.Clients.Clients(p.ConnectionId).SendAsync("UpdateTable", gamePrint);
                    }
                }
            }

            return Json(true);
        }

    }
}
