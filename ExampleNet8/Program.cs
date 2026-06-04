using BlazorAccessControl.EFCore;
using BlazorAccessControl.Interface;
using ExampleNet8.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExampleNet8
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


            builder.Services.AddDbContextFactory<MyDBContext>(lifetime: ServiceLifetime.Transient);

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<MyDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserService, DummyUserService>();

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

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            
            DummyUserService.MapLoginUrl(app);
            app.SetRequestLocalization();
            app.MapGet("/setlanguage", LanguageHelper.SetLanguage);

            app.Run();
        }
    }
}
