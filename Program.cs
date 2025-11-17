using barberchainAPI.Data;
using barberchainAPI.Functional;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

namespace barberchainAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContext<BarberchainDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.MapEnum<AccountRole>("account_role")));
            builder.Services.AddMudServices();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient("server", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7027/");
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(x =>
                {
                    x.Cookie.Name = "authCookie";
                    x.LoginPath = "/auth";
                });
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            builder.Services.AddScoped<LocalStorageService>();
            builder.Services.AddScoped<CartService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapControllers();

            app.MapRazorComponents<barberchainAPI.Components.App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
