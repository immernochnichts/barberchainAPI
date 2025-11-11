using barberchainAPI.Components;
using barberchainAPI.Data;
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

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContext<BarberchainDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.MapEnum<AccountRole>("account_role")));
            builder.Services.AddMudServices();
            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/auth";     // redirect for unauthorized users
                    options.AccessDeniedPath = "/forbidden";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

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
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapRazorComponents<barberchainAPI.Components.App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
