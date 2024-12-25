using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RotaryTheGame.Models;
using RotaryTheGame.SignalHub;

namespace RotaryTheGame.Controllers
{
    public class LobbyController : Controller
    {
        private readonly GamesHandler _gamesHandler;
        private readonly IHubContext<GamesHub> _hubContext;

        public LobbyController(GamesHandler gamesHandler, IHubContext<GamesHub> hubContext)
        {
            _gamesHandler = gamesHandler;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Wait(string id)
        {
            Player? player = _gamesHandler.getPlayerByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            if (player == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Dictionary<string, object>>? playersList = _gamesHandler.getPlayerListByGameCode(id, HttpContext.Request.Cookies["uuid"]);

            if (playersList == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if(_gamesHandler.getGameByCode(id).Status != "waiting")
            {
                return RedirectToAction("Play", "Game", new { id = id });
            }

            ViewData["isHost"] = player.Host;
            ViewData["code"] = id;

            return View(playersList);
        }

        [HttpDelete]
        public IActionResult DeletePlayer(string id, int i)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game != null)
            {
                Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

                if (player != null)
                {
                    if (player.Host && game.Status == "waiting")
                    {
                        _hubContext.Clients.Clients(game.Players[i].ConnectionId).SendAsync("RedirectTo", "/");
                        game.Players.RemoveAt(i);

                        List<Dictionary<string, object>> playerList = game.getPlayerList();

                        foreach (Player p in game.Players)
                        {
                            _hubContext.Clients.Clients(p.ConnectionId).SendAsync("PlayerJoined", playerList);
                        }

                        return Json(true);
                    }
                }
            }

            return Json(false);
        }

        [HttpDelete]
        public IActionResult DeleteGame(string id)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game != null)
            {
                Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

                if (player != null)
                {
                    if (player.Host && game.Status == "waiting")
                    {
                        foreach (Player p in game.Players)
                        {
                            _hubContext.Clients.Clients(p.ConnectionId).SendAsync("RedirectTo", "/");
                        }

                        _gamesHandler.Games.RemoveAll(game => game.Code == id);
     
                        return Json(true);
                    }
                }
            }

            return Json(false);
        }

        [HttpPost]
        public IActionResult StartGame(string id)
        {
            Game? game = _gamesHandler.getGameByCode(id);

            if (game != null)
            {
                Player? player = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);

                if (player != null)
                {
                    if (player.Host && game.Status == "waiting")
                    {
                        game.startGame();

                        foreach (Player p in game.Players)
                        {
                            _hubContext.Clients.Clients(p.ConnectionId).SendAsync("RedirectTo", "/Game/Play/"+id);
                        }

                        return Json(true);
                    }
                }
            }

            return Json(false);
        }
    }
}
