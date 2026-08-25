using BlazorAccessControl.EFCore;
using BlazorAccessControl.Interface;
using ExampleNet10.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
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
                //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                //    options =>
                //    {
                //        var appBasePath = builder.Configuration.GetSection("AppBasePath").Value?.TrimEnd('/');
                //        if (!string.IsNullOrWhiteSpace(appBasePath) && !appBasePath.StartsWith("/"))
                //        {
                //            appBasePath = "/" + appBasePath;
                //        }
                //        options.Cookie.Name = $".AspNetCore.Identity.Application{typeof(App).Namespace}";
                //        //options.Cookie.Path = appBasePath;
                //        options.LoginPath = appBasePath + "/account/signin";
                //        options.LogoutPath = appBasePath + builder.Configuration.GetSection("Authentication:EndPoint_Signout").Value ?? "/account/signout";
                //        options.ForwardSignIn = appBasePath + "/account/signin";
                //    })
                .AddIdentityCookies(option => {
                    option.ApplicationCookie?.Configure(options => {
                        //var appBasePath = builder.Configuration.GetSection("AppBasePath").Value?.TrimEnd('/');
                        //if (!string.IsNullOrWhiteSpace(appBasePath) && !appBasePath.StartsWith("/"))
                        //{
                        //    appBasePath = "/" + appBasePath;
                        //}
                        options.Cookie.Name = $".AspNetCore.Identity.Application{typeof(Program).Namespace}";
                        //options.Cookie.Path = appBasePath;
                        options.LoginPath = "/account/signin"; //route of Login.razor
                        //options.Events.OnRedirectToLogin = context =>
                        //{
                        //    context.Response.Redirect(appBasePath + "/account/signin?returnurl=" + context.Request.GetEncodedUrl());
                        //    return Task.CompletedTask;
                        //};
                        //options.LogoutPath = appBasePath + builder.Configuration.GetSection("Authentication:EndPoint_Signout").Value ?? "/account/signout";
                        options.AccessDeniedPath = "/access-denied";
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
            //sub-path
            //app.UsePathBase($"/{app.Configuration.GetValue<string>("AppBasePath")}");
            app.UseAuthorization();
            app.UseAntiforgery();
            //app.MapBlazorHub("/staging/_blazor");
            //sub-path

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();


            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode(o=>o.DisableWebSocketCompression = true);

            DummyUserServiceGuid.MapLoginUrl(app);
            app.SetRequestLocalization();
            app.MapGet("/setlanguage", LanguageHelper.SetLanguage);
            //using (var serviceScope = app.Services.CreateScope())
            //{
            //    var services = serviceScope.ServiceProvider;
            //    var userService = services.GetRequiredService<IUserService>();
            //    app.MapGet("/login_password", userService.PasswordSignIn );
            //}
            app.UseRewriter();
            app.Run();
        }
    }
}
