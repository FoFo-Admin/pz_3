using RotaryTheGame.Models;
using RotaryTheGame.SignalHub;

namespace RotaryTheGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<GamesHandler>();


            //builder.WebHost.ConfigureKestrel(serverOptions =>
            //{
            //    serverOptions.Listen(System.Net.IPAddress.Any, 5000); // HTTP
            //    serverOptions.Listen(System.Net.IPAddress.Any, 5001, listenOptions => listenOptions.UseHttps()); // HTTPS
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<GamesHub>("/gamesHub");

            app.Run();
        }
    }
}
