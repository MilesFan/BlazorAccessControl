using BlazorAccessControl.EFCore;
using BlazorAccessControl.Interface;
using ExampleNet8;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public class DummyUserService : IUserService
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
        var userService = services.GetRequiredService<IUserService>();
        var endpoint_password = userService.GetPasswordLoginEndPoint();
        if (!string.IsNullOrEmpty(endpoint_password))
        {
            app.MapPost(endpoint_password, async (HttpContext context, [FromForm] string UserName, [FromForm] string Password, [FromServices] IUserService userService) =>
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
            app.MapPost(endpoint_signout, async (HttpContext context, [FromForm] string? ReturnUrl, [FromServices] IUserService userService) =>
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
            app.MapPost(endpoint_oauthvalidation, async (IUserService _userService, IConfiguration _config, HttpContext context, [FromQuery] string? ReturnUrl, [FromForm] string __jwt_token, [FromServices] IUserService userService) =>
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
    private IUser? _currentUser;
    //private MyDBContext context;
    private readonly IDbContextFactory<MyDBContext> contextFactory;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signinManager;
    private readonly IAntiforgery antiforgery;
    private readonly IHttpContextAccessor? httpContextAccessor;
    private readonly IConfiguration config;
    public DummyUserService(
        IDbContextFactory<MyDBContext> _contextFactory,
        UserManager<ApplicationUser> _userManager,
        SignInManager<ApplicationUser> _signinManager,
        IAntiforgery _antiforgery,
        IConfiguration _config,
        IHttpContextAccessor? _httpContextAccessor)
    {
        contextFactory = _contextFactory;
        userManager = _userManager;
        signinManager = _signinManager;
        antiforgery = _antiforgery;
        httpContextAccessor = _httpContextAccessor;
        config = _config;
    }
    public IUser? CurrentUser
    {
        get
        {
            if (_currentUser is not null) return _currentUser;
            try
            {
                if (!signinManager.IsSignedIn(signinManager.Context.User)) return null;
                using var context = contextFactory.CreateDbContext();
                var userId = userManager.GetUserId(signinManager.Context.User);
                _currentUser = context.Users
                                      .Include(i => i.UserRoles).ThenInclude(j=>j.Role)
                                      .Include(i => i.Claims)
                                      .FirstOrDefault(i => i.Id == userId);
                return _currentUser;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }

    public async Task CreateUserAsync(IUser user)
    {
        var _user = user as ApplicationUser;
        if (_user == null) throw new ArgumentException("Invalid user type");
        _user.NormalizedUserName = _user.UserName?.ToUpper();
        _user.NormalizedEmail = _user.Email?.ToUpper();
        using var context = await contextFactory.CreateDbContextAsync();
        if (_user.UserRoles.Count() > 0)
        {
            foreach (var userRole in _user.UserRoles)
            {
                context.Attach(userRole).State = EntityState.Unchanged;
            }
            context.UserRoles.AddRange(_user.UserRoles);
        }
        if (_user.Claims.Count() > 0)
        {
            foreach (var userClaim in _user.Claims)
            {
                context.Attach(userClaim).State = EntityState.Unchanged;
            }
            context.UserClaims.AddRange(_user.Claims);
        }
        await context.Users.AddAsync(_user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUserByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ICollection<IRole>> GetAllRolesAsync()
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var roles = await context.Roles.AsNoTracking().ToArrayAsync();
        return roles;
    }

    public async Task<ICollection<IUser>> GetAllUsersAsync()
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var users = await context.Users.AsNoTracking()
                                        .Include(i => i.UserRoles)
                                        .ThenInclude(i => i.Role)
                                        .Include(i => i.Claims)
                                        .ToArrayAsync();
        return users;
    }

    public async Task<ICollection<IRole>> GetUserRolesAsync(IUser user)
    {
        var roles = user.GetRoles();
        return roles;
    }
    public async Task<IRole?> GetRoleByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.Roles.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IUser?> GetUserByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.AsNoTracking()
                                    .Include(i => i.UserRoles)
                                    .ThenInclude(i => i.Role)
                                    .Include(i => i.Claims)
                                    .FirstOrDefaultAsync(i => i.Id == id);
    }
    public async Task<IUser?> GetUserByNameAsync(string UserName)
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
        var _user = user as ApplicationUser;
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
        try
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

    }
    public Task SignInAsync(Uri ExternalUrl)
    {
        throw new NotImplementedException();
    }
    public Task SignOutAsync(IUser user)
    {
        throw new NotImplementedException();
    }
    public async Task SignOutAsync()
    {
        await signinManager.SignOutAsync();
    }

    public async Task UpdateUserAsync(IUser user)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var userInDB = await context.Users.AsTracking()
                                                .Include(i => i.UserRoles)
                                                .Include(i => i.Claims)
                                                .FirstOrDefaultAsync(i => i.Id == user.Id);

            if (userInDB == null)
                throw new Exception("User not found");

            userInDB.UserName = user.UserName;
            userInDB.NormalizedUserName = user.UserName?.ToUpper();
            userInDB.Email = user.Email;
            userInDB.NormalizedEmail = user.Email?.ToUpper();

            var newUserRoles = user.GetRoles().ToArray();
            var rolesInDB = userInDB.UserRoles.ToArray();

            userInDB.UserRoles.RemoveAll(i => newUserRoles.Any(r => r.Id == i.RoleId) == false);

            userInDB.UserRoles.AddRange(newUserRoles.Where(i => rolesInDB.Any(r => r.RoleId == i.Id) == false)
                                                    .Select(i => new ApplicationUserRole
                                                    {
                                                        RoleId = i.Id,
                                                        UserId = user.Id
                                                    }
                                                            )
                                        );

            var newUserClaims = user.GetClaims().ToArray();
            var claimsInDB = userInDB.Claims.ToArray();
            var claimsToRemove = userInDB.Claims.Where(i => newUserClaims.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false);
            userInDB.Claims.RemoveAll(i => newUserClaims.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false);
            var claimsToAdd = newUserClaims.Where(i => claimsInDB.Any(r => r.ClaimType == i.ClaimType && r.ClaimValue == i.ClaimValue) == false)
                                            .Select(i => new ApplicationUserClaim
                                            {
                                                UserId = user.Id,
                                                ClaimType = i.ClaimType,
                                                ClaimValue = i.ClaimValue
                                            }
                                                    );
            userInDB.Claims.AddRange(claimsToAdd);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public async Task UpdateUserAsync_old(IUser user)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var _user = user as ApplicationUser;
            if (_user == null) throw new ArgumentException("Invalid user type");
            await context.Users.AsNoTracking().Where(u => u.Id == user.Id).ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(u => u.UserName, user.UserName)
                    .SetProperty(u => u.NormalizedUserName, user.UserName ==null ? null : user.UserName.ToUpper())
                    //.SetProperty(u => u.DisplayName, user.DisplayName)
                    .SetProperty(u => u.Email, user.Email)
                    .SetProperty(u => u.NormalizedEmail, user.Email ==null ? null : user.Email.ToUpper())
            );

            var currentUserRoles = _user.UserRoles.ToArray();// user.GetRoles();
            var existingsUserRoles = await context.UserRoles.AsNoTracking().Where(i => i.UserId == user.Id).ToListAsync();

            var rolesToAdd = currentUserRoles.Where(i => existingsUserRoles.Any(r => r.RoleId == i.RoleId) == false)
                                                .Select(i => new ApplicationUserRole { RoleId = i.RoleId, UserId = user.Id });
            if (rolesToAdd.Count() > 0)
            {
                foreach (var role in rolesToAdd)
                {
                    context.Attach(role).State = EntityState.Added;
                }
            }

            var rolesToRemove = existingsUserRoles.Where(i => currentUserRoles.Any(r => r.RoleId == i.RoleId) == false);
            if (rolesToRemove.Count() > 0)
            {
                foreach (var role in rolesToRemove)
                {
                    context.Attach(role).State = EntityState.Deleted;
                }
            }


            var currentUserClaims = _user.Claims.ToArray();
            var existingsUserClaims = await context.UserClaims.AsNoTracking().Where(i => i.UserId == user.Id).ToListAsync();

            var claimsToAdd = currentUserClaims.Where(i => existingsUserClaims.Any(r => r.Id == i.Id) == false);
            if (claimsToAdd.Count() > 0)
            {
                foreach (var claim in claimsToAdd)
                {
                    context.UserClaims.Add(new ApplicationUserClaim { Id = claim.Id, ClaimType = claim.ClaimType, ClaimValue = claim.ClaimValue, UserId = claim.UserId });
                    //context.Attach(claim).State = EntityState.Added;
                    //if (claim.User is not null) context.Attach(claim.User).State = EntityState.Added;
                }
            }

            var claimsToRemove = existingsUserClaims.Where(i => currentUserClaims.Any(r => r.Id == i.Id) == false);
            if (claimsToRemove.Count() > 0)
            {
                foreach (var claim in claimsToRemove)
                {
                    context.Attach(claim).State = EntityState.Deleted;
                }
            }

            foreach (var claimToUpdate in existingsUserClaims)
            {
                var claim = currentUserClaims.FirstOrDefault(j => j.Id == claimToUpdate.Id);
                if (claim is null || claim.ClaimValue == claimToUpdate.ClaimValue) continue;
                context.Attach(claim).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
    public async Task CreateRoleAsync(IRole role)
    {
        var _role = role as ApplicationRole;
        if (_role == null) throw new ArgumentException("Invalid role type");
        using var context = await contextFactory.CreateDbContextAsync();
        _role.NormalizedName = role.Name?.ToUpper();
        await context.Roles.AddAsync(_role);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRoleAsync(IRole role)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Roles.Where(r => r.Id == role.Id).ExecuteUpdateAsync(setters =>
            setters
                .SetProperty(u => u.Name, role.Name)
                .SetProperty(u => u.NormalizedName, role.Name ==null ? null : role.Name.ToUpper())
        );
    }

    public async Task DeleteRoleByIdAsync(string id)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        await context.Roles.Where(r => r.Id == id).ExecuteDeleteAsync();
    }
    public async Task ChangePasswordAsync(IUser user, string oldPassword, string newPassword)
    {
        var _user = user as ApplicationUser;
        if (_user == null) throw new ArgumentException("Invalid user type");
        await userManager.ChangePasswordAsync(_user, oldPassword, newPassword);
    }
    public async Task ResetPasswordAsync(IUser user, string newPassword)
    {
        //var _user = user as ApplicationUser;
        var _user = await userManager.FindByIdAsync(user.Id);
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
        return antiforgery?.GetAndStoreTokens(httpContext).RequestToken ?? string.Empty;
    }

}