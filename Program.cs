using barberchainAPI.Data;
using barberchainAPI.Functional;
using barberchainAPI.Functional.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

namespace barberchainAPI
{
    public class Program
    {
        /*
        TODO:
        1. fix schedule update on order placement and cancellation
        */

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContext<BarberchainDbContext>(options => options.EnableSensitiveDataLogging().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                o =>
                {
                    o.MapEnum<AccountRole>("account_role");
                    o.MapEnum<OrderMethod>("order_method");
                    o.MapEnum<OrderStatus>("order_status");
                    o.MapEnum<ScheduleRequestStatus>("schedule_request_status");
                    o.MapEnum<ActionType>("action_type");
                    o.MapEnum<NotificationType>("notification_type");
                }
            ));

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
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<LocalStorageService>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddHostedService<OrderExpirationService>();
            builder.Services.AddHostedService<OrderUpdaterService>();
            builder.Services.AddScoped<IScheduleRequestService, ScheduleRequestService>();
            builder.Services.AddScoped<ICreateScheduleService, CreateScheduleService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IMapService, MapService>();

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
