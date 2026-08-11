using BlazorAccessControl.EFCore;
using BlazorAccessControl.Interface;
using ExampleNet10;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public class DummyUserServiceULID: IUserService<string>
{
    public string? GetPasswordLoginEndPoint() => config.GetValue<string?>("Authentication:EndPoint_Password");
    public string? GetSignOutEndPoint() => config.GetValue<string?>("Authentication:EndPoint_Signout");
    public string? GetSignOutPoint() => config.GetValue<string?>("Authentication:EndPoint_Password");
    public string? GetOAuthAuthenticationEndPoint() => config.GetValue<string?>("Authentication:EndPoint_OAuthAuthentication");
    public string? GetOAuthValidationEndPoint() => config.GetValue<string?>("Authentication:EndPoint_OAuthValidation");
    public static void MapLoginUrl(WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        var services = serviceScope.ServiceProvider;
        var userService = services.GetRequiredService<IUserService<Guid>>();
        var endpoint_password = userService.GetPasswordLoginEndPoint();
        var pathBase = app.Configuration.GetValue<string>("AppBasePath")?.TrimEnd('/') ?? "";
        if (!string.IsNullOrEmpty(endpoint_password))
        {
            app.MapPost($"{pathBase}{endpoint_password}", async (HttpContext context, [FromForm] string UserName, [FromForm] string Password, [FromServices] IUserService<Guid> userService) =>
            {
                try
                {
                    await userService.PasswordSignIn(UserName, Password);
                    return Results.Ok(new { Succeeded = true });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { Succeeded = false, Message = "Login Failed: " + ex.Message });
                }
            });
        }
        var endpoint_signout = userService.GetSignOutEndPoint();
        if (!string.IsNullOrEmpty(endpoint_signout))
        {
            app.MapPost($"{pathBase}{endpoint_signout}", async (HttpContext context, [FromForm] string? ReturnUrl, [FromServices] IUserService<Guid> userService) =>
            {
                var _ReturnUrl = ReturnUrl ?? context.Request.Headers.Referer.ToString();
                if (string.IsNullOrEmpty(_ReturnUrl)) _ReturnUrl = "/";
                try
                {
                    await userService.SignOutAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return Results.Redirect(_ReturnUrl);
            });
        }
        var endpoint_oauthvalidation = userService.GetOAuthValidationEndPoint();
        if (!string.IsNullOrEmpty(endpoint_oauthvalidation))
        {
            app.MapPost($"{pathBase}{endpoint_oauthvalidation}", async (IUserService<Guid> _userService, IConfiguration _config, HttpContext context, [FromQuery] string? ReturnUrl, [FromForm] string __jwt_token, [FromServices] IUserService<Guid> userService) =>
            {
                var _ReturnUrl = ReturnUrl;
                if (string.IsNullOrEmpty(_ReturnUrl)) _ReturnUrl = "/";
                if (string.IsNullOrWhiteSpace(__jwt_token))
                {
                    return Results.BadRequest("JWT Token Missing");
                }
                try
                {
                    await userService.SignInWithTokenAsync(__jwt_token);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                return Results.Redirect(_ReturnUrl);
            }).DisableAntiforgery();
        }
    }
    private IUser<string>? _currentUser;
    private readonly IDbContextFactory<MyDBContext<string>> contextFactory;
    private readonly UserManager<ApplicationUser<string>> userManager;
    private readonly RoleManager<ApplicationRole<string>> roleManager;
    private readonly SignInManager<ApplicationUser<string>> signinManager;
    private readonly IAntiforgery antiforgery;
    private readonly IHttpContextAccessor? httpContextAccessor;
    private readonly IConfiguration config;
    public DummyUserServiceULID(
        IDbContextFactory<MyDBContext<string>> _contextFactory,
        UserManager<ApplicationUser<string>> _userManager,
        RoleManager<ApplicationRole<string>> _roleManager,
        SignInManager<ApplicationUser<string>> _signinManager,
        IAntiforgery _antiforgery,
        IConfiguration _config,
        IHttpContextAccessor? _httpContextAccessor)
    {
        contextFactory = _contextFactory;
        userManager = _userManager;
        roleManager = _roleManager;
        signinManager = _signinManager;
        antiforgery = _antiforgery;
        httpContextAccessor = _httpContextAccessor;
        config = _config;
    }
    public IUser<string>? CurrentUser
    {
        get
        {
            if (_currentUser is not null) return _currentUser;
            try
            {
                if (!signinManager.IsSignedIn(signinManager.Context.User)) return null;
                using var context = contextFactory.CreateDbContext();
                var userName = userManager.GetUserName(signinManager.Context.User);
                _currentUser = context.Users
                                      .Include(i => i.UserRoles).ThenInclude(j=>j.Role)
                                      .Include(i => i.Claims)
                                      .FirstOrDefault(i => i.UserName == userName);
                return _currentUser;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }

    public async Task CreateUserAsync(IUser<string> user)
    {   
        var userId = NewUserId();
        var _user = new ApplicationUser<string>
        {
            Id = userId,
            UserName = user.UserName,
            NormalizedUserName = user.UserName!.ToUpper(),
            Email = user.Email,
            NormalizedEmail = user.Email!.ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            SecurityStamp = Guid.NewGuid().ToString("D"),
            Claims = user.GetClaims().Select(i=>
                new ApplicationUserClaim<string>
                {
                    Id = NewUserClaimId(),
                    UserId = userId,
                    ClaimType = i.ClaimType,
                    ClaimValue = i.ClaimValue,
                }).ToList(),
            UserRoles = user.GetRoles().Select(i=>
            new ApplicationUserRole<string>
            {
                RoleId = i.Id,
                UserId = userId,
            }).ToList()
        };
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Users.AddAsync(_user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUserByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Users.Where(u => u.Id.Equals(id)).ExecuteDeleteAsync();
    }

    public async Task<ICollection<IRole<string>>> GetAllRolesAsync()
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var roles = await context.Roles.AsNoTracking().ToArrayAsync();
        return roles;
    }

    public async Task<ICollection<IUser<string>>> GetAllUsersAsync()
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var users = await context.Users.AsNoTracking()
                                        .Include(i => i.UserRoles)
                                        .ThenInclude(i => i.Role)
                                        .Include(i => i.Claims)
                                        .ToArrayAsync();
        return users;
    }

    public async Task<ICollection<IRole<string>>> GetUserRolesAsync(IUser<string> user)
    {
        var roles = user.GetRoles();
        return roles;
    }
    public async Task<IRole<string>?> GetRoleByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.Roles.AsNoTracking().FirstOrDefaultAsync(i => i.Id.Equals(id));
    }

    public async Task<IUser<string>?> GetUserByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking()
                                    .Include(i => i.UserRoles)
                                    .ThenInclude(i => i.Role)
                                    .Include(i => i.Claims)
                                    .FirstOrDefaultAsync(i => i.Id.Equals(id));
    }
    public async Task<IUser<string>?> GetUserByNameAsync(string UserName)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking()
                                    .Include(i => i.UserRoles)
                                    .ThenInclude(i => i.Role)
                                    .Include(i => i.Claims)
                                    .FirstOrDefaultAsync(i => i.UserName == UserName);
    }

    public Task SetPasswordAsync(string id, string Password)
    {
        throw new NotImplementedException();
    }

    public async Task SignInAsync(string UserName)
    {
        var user = await GetUserByNameAsync(UserName);
        var _user = user as ApplicationUser<string>;
        if (_user == null) throw new Exception("User not found");
        await signinManager.SignInAsync(_user, true);
        _currentUser = user;
    }

    public async Task SignInAsync(string UserName, string Password)
    {
        try
        {
            var result = await signinManager.PasswordSignInAsync(UserName, Password, true, true);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(UserName);
                _currentUser = user;
                if (_currentUser is null)
                {
                    throw new Exception("User not found after successful login");
                }
            }
            else
            {
                throw new Exception(result.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

    }
    public async Task SignInWithTokenAsync(string token)
    {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(config["Authentication:Jwt:IssuerSigningKey"]!), out _);
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ValidIssuer = config["Authentication:Jwt:Issuer"],
            ValidAudience = config["Authentication:Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa),
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userName = jwtToken.Claims.First(x => x.Type == "UserAccount").Value;
        userName = Regex.Replace(userName, @"^AD101\\", "");

        if (!string.IsNullOrEmpty(userName))
        {
            await SignInAsync(userName);
        }
        else
        {
            throw new Exception("Unauthorized");
        }
    }
    public Task SignInAsync(Uri ExternalUrl)
    {
        throw new NotImplementedException();
    }
    public Task SignOutAsync(IUser<string> user)
    {
        throw new NotImplementedException();
    }
    public async Task SignOutAsync()
    {
        await signinManager.SignOutAsync();
    }

    public async Task UpdateUserAsync(IUser<string> user)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var userInDB = await context.Users.AsTracking()
                                            .Include(i => i.UserRoles)
                                            .Include(i => i.Claims)
                                            .FirstOrDefaultAsync(i => i.Id.Equals(user.Id));
        if (userInDB == null)
            throw new Exception("User not found");

        userInDB.UserName = user.UserName;
        userInDB.NormalizedUserName = user.UserName?.ToUpper();
        userInDB.Email = user.Email;
        userInDB.NormalizedEmail = user.Email?.ToUpper();
        userInDB.ConcurrencyStamp = Guid.NewGuid().ToString("D");

        var newUserRoles = user.GetRoles().ToArray();
        var rolesInDB = userInDB.UserRoles.ToArray();

        int countRolesRemoved = userInDB.UserRoles.RemoveAll(i => newUserRoles.Any(r => r.Id.Equals(i.RoleId)) == false);

        var rolesToAdd = newUserRoles.Where(i => rolesInDB.Any(r => r.RoleId.Equals(i.Id)) == false)
                                                .Select(i => new ApplicationUserRole<string>
                                                {
                                                    RoleId = i.Id,
                                                    UserId = user.Id
                                                }
                                            );
        int countRolesAdded = rolesToAdd.Count();
        userInDB.UserRoles.AddRange(rolesToAdd);

        var newUserClaims = user.GetClaims().ToArray();
        var claimsInDB = userInDB.Claims.ToArray();
        var claimsToRemove = userInDB.Claims.Where(i => newUserClaims.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false);
        userInDB.Claims.RemoveAll(i => newUserClaims.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false);
        var claimsToAdd = newUserClaims.Where(i => claimsInDB.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false)
                                        .Select(i => new ApplicationUserClaim<string>
                                        {
                                            Id = NewUserClaimId(),
                                            UserId = user.Id,
                                            ClaimType = i.ClaimType,
                                            ClaimValue = i.ClaimValue
                                        }
                                                );
        userInDB.Claims.AddRange(claimsToAdd);
            
        if (countRolesRemoved>0 || countRolesAdded>0)
        {
            userInDB.SecurityStamp = Guid.NewGuid().ToString("D");
        }
        await context.SaveChangesAsync();
    }

    public async Task CreateRoleAsync(IRole<string> role)
    {
        var _role = role as ApplicationRole<string>;
        if (_role == null) throw new ArgumentException("Invalid role type");
        _role.Id = NewRoleId();
        _role.ConcurrencyStamp = Guid.NewGuid().ToString("D");
        await roleManager.CreateAsync(_role);
    }

    public async Task UpdateRoleAsync(IRole<string> role)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Roles.Where(r => r.Id.Equals(role.Id)).ExecuteUpdateAsync(setters =>
            setters
                .SetProperty(u => u.Name, role.Name)
                .SetProperty(u => u.NormalizedName, role.Name ==null ? null : role.Name.ToUpper())
                .SetProperty(u => u.ConcurrencyStamp, Guid.NewGuid().ToString("D"))
        );
    }

    public async Task DeleteRoleByIdAsync(string id)
    {
        var role = await roleManager.Roles.FirstOrDefaultAsync(i=>i.Id.Equals(id));
        if (role==null) throw new Exception("Role Not Found");
        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
        foreach (var user in usersInRole)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }
        await roleManager.DeleteAsync(role);
    }
    public async Task ChangePasswordAsync(IUser<string> user, string oldPassword, string newPassword)
    {
        var _user = user as ApplicationUser<string>;
        if (_user == null) throw new ArgumentException("Invalid user type");
        await userManager.ChangePasswordAsync(_user, oldPassword, newPassword);
    }
    public async Task ResetPasswordAsync(IUser<string> user, string newPassword)
    {
        var _user = await userManager.FindByNameAsync(user.UserName ?? throw new Exception("User Name is empty"));
        if (_user == null) throw new ArgumentException("Invalid user type");
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(_user);
        var result = await userManager.ResetPasswordAsync(_user, resetToken, newPassword);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(i => i.Description)));
        }
    }

    public async Task PasswordSignIn(string UserName, string Password)
    {
        try
        {
            var result = await signinManager.PasswordSignInAsync(UserName, Password, true, true);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(UserName);
                _currentUser = user;
                if (_currentUser is null)
                {
                    throw new Exception("User Not Found");
                }
            }
            else if (result.IsLockedOut)
            {
                throw new Exception("Account Locked");
            }
            else if (result.IsNotAllowed)
            {
                throw new Exception("Not Allowed");
            }
            else
            {
                throw new Exception("Incorrect User Name or Password");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    public string GetAntiForgeryToken()
    {
        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext == null) return string.Empty;
        return antiforgery?.GetTokens(httpContext).RequestToken ?? string.Empty;
    }
    public string NewUserId()
    {
        return Ulid.NewUlid().ToString();
    }
    public string NewUserClaimId()
    {
        return Ulid.NewUlid().ToString();
    }
    public string NewRoleId()
    {
        return Ulid.NewUlid().ToString();
    }
    //public IClaim<string> NewUserClaim(string ClaimType, string ClaimValue)
    //{
    //    return new ApplicationUserClaim<string>
    //    {
    //        Id = Ulid.NewUlid().ToString(),
    //        ClaimType = ClaimType,
    //        ClaimValue = ClaimValue
    //    };
    //}
}