using BlazorAccessControl.EFCore;
using BlazorAccessControl.Interface;
using ExampleNet10.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ExampleNet10
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services
                .AddCascadingAuthenticationState()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.LoginPath = "/account/signin";
                        options.LogoutPath = "/account/signout";
                        options.ForwardSignIn = "/account/signin";
                    })
                .AddIdentityCookies(option => {
                    option.ApplicationCookie?.Configure(s => {
                        s.LoginPath = "/account/signin";
                        s.LogoutPath = "/account/signout";
                        s.AccessDeniedPath ="/access-denied";
                    });
                });

            builder.Services.AddDbContextFactory<MyDBContext<string>>(lifetime: ServiceLifetime.Transient);

            builder.Services.AddIdentityCore<ApplicationUser<string>>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<ApplicationRole<string>>()
                .AddEntityFrameworkStores<MyDBContext<string>>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserService<string>, DummyUserServiceULID>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            DummyUserServiceULID.MapLoginUrl(app);
            app.SetRequestLocalization();
            app.MapGet("/setlanguage", LanguageHelper.SetLanguage);
            //using (var serviceScope = app.Services.CreateScope())
            //{
            //    var services = serviceScope.ServiceProvider;
            //    var userService = services.GetRequiredService<IUserService>();
            //    app.MapGet("/login_password", userService.PasswordSignIn );
            //}
            app.Run();
        }
    }
}
