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
                //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                //    options =>
                //    {
                //        var appBasePath = builder.Configuration.GetSection("AppBasePath").Value?.TrimEnd('/');
                //        if (!string.IsNullOrWhiteSpace(appBasePath) && !appBasePath.StartsWith("/"))
                //        {
                //            appBasePath = "/" + appBasePath;
                //        }
                //        options.Cookie.Path = appBasePath;
                //        options.LoginPath = appBasePath + "/account/signin";
                //        options.LogoutPath = appBasePath + builder.Configuration.GetSection("Authentication:EndPoint_Signout").Value ?? "/account/signout";
                //        options.ForwardSignIn = appBasePath + "/account/signin";
                //    })
                .AddIdentityCookies(option => {
                    option.ApplicationCookie?.Configure(options => {
                        var appBasePath = builder.Configuration.GetSection("AppBasePath").Value?.TrimEnd('/');
                        if (!string.IsNullOrWhiteSpace(appBasePath) && !appBasePath.StartsWith("/"))
                        {
                            appBasePath = "/" + appBasePath;
                        }
                        //options.Cookie.Path = appBasePath;
                        options.LoginPath = appBasePath + "/account/signin";
                        options.LogoutPath = appBasePath + builder.Configuration.GetSection("Authentication:EndPoint_Signout").Value ?? "/account/signout";
                        options.AccessDeniedPath = appBasePath + "/access-denied";
                    });
                });


            builder.Services.AddDbContextFactory<MyDBContext<Guid>>(lifetime: ServiceLifetime.Transient);

            builder.Services.AddIdentityCore<ApplicationUser<Guid>>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<ApplicationRole<Guid>>()
                .AddEntityFrameworkStores<MyDBContext<Guid>>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserService<Guid>, DummyUserServiceGuid>();

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
            
            DummyUserServiceGuid.MapLoginUrl(app);
            app.SetRequestLocalization();
            app.MapGet("/setlanguage", LanguageHelper.SetLanguage);

            app.Run();
        }
    }
}
