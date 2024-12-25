using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RotaryTheGame.Models;
using RotaryTheGame.SignalHub;
using System.Diagnostics;

namespace RotaryTheGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly GamesHandler _gamesHandler;
        private readonly IHubContext<GamesHub> _hubContext;

        public HomeController(GamesHandler gamesHandler, IHubContext<GamesHub> hubContext)
        {
            _gamesHandler = gamesHandler;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["gamesCount"] = _gamesHandler.Games.Count();
            return View();
        }

        [HttpPost]
        public IActionResult Index(Player player, string? code, string? codeSubmit, string? gameSubmit)
        {
            if(ModelState.IsValid)
            {
                if (gameSubmit != null)
                {
                    Game newGame = _gamesHandler.generateNewGame();
                    player = newGame.addPlayer(player);
                    player.setHost();

                    HttpContext.Response.Cookies.Append(
                            "uuid",
                            player.Uuid,
                            new CookieOptions { HttpOnly = true }
                        );

                    return RedirectToAction("Wait", "Lobby", new { id = newGame.Code });
                }
                else if (codeSubmit != null)
                {
                    if (code == null || code == string.Empty)
                        ModelState.AddModelError("", "Ви не ввели код кімнати");
                    else if (code.Length != 4)
                        ModelState.AddModelError("", "Довжина коду повинна дорівнювати 4 символам");
                    else
                    {
                        Game? game = _gamesHandler.getGameByCode(code);
                        if (game != null)
                        {
                            Player? alreadyPlayer = game.getPlayerByUuid(HttpContext.Request.Cookies["uuid"]);
                            if (alreadyPlayer == null)
                            {
                                if (game.Status == "waiting")
                                {
                                    player = game.addPlayer(player);

                                    HttpContext.Response.Cookies.Append(
                                        "uuid",
                                        player.Uuid,
                                        new CookieOptions { HttpOnly = true }
                                    );

                                    List<Dictionary<string, object>> playerList = game.getPlayerList();

                                    foreach (Player p in game.Players)
                                    {
                                        _hubContext.Clients.Clients(p.ConnectionId).SendAsync("PlayerJoined", playerList);
                                    }

                                    return RedirectToAction("Wait", "Lobby", new { id = code });
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Гра вже почалась");
                                }
                            }
                            else
                            {
                                return RedirectToAction("Wait", "Lobby", new { id = code });
                            }
                            
                        }
                        else
                        {
                            ModelState.AddModelError("", "Кімнати не існує");
                        }
                    }
                }
            }
            ViewData["gamesCount"] = _gamesHandler.Games.Count();
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
