using BlazorAccessControl.Interface;
using Example.Components.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Example
{    
    public class DummyUserService : IUserService
    {
        private IUser _currentUser;
        //private DBContext context;
        private readonly IDbContextFactory<DBContext> contextFactory;
        private readonly UserManager<ApplicationUser> userManager;
        public DummyUserService(IDbContextFactory<DBContext> _contextFactory, UserManager<ApplicationUser> _userManager)
        {
            _currentUser = new ApplicationUser
            {
                UserName = "john.doe",
                DisplayName = "John Doe",
                Email = "doe.john@example.com"
            };
            contextFactory = _contextFactory;
            userManager = _userManager;
        }
        public IUser? CurrentUser => _currentUser;

        public async Task CreateUserAsync(IUser user)
        {
            var _user = user as ApplicationUser;
            if (_user == null) throw new ArgumentException("Invalid user type");
            _user.NormalizedUserName = _user.UserName?.ToUpper();
            _user.NormalizedEmail = _user.Email?.ToUpper();
            using var context = await contextFactory.CreateDbContextAsync();
            if (_user.UserRoles.Count()>0)
            {
                foreach(var userRole in _user.UserRoles)
                {
                    context.Attach(userRole).State = EntityState.Unchanged;
                }
                context.UserRoles.AddRange(_user.UserRoles);
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
            var roles = await context.Roles.ToArrayAsync();
            return roles;
        }

        public async Task<ICollection<IUser>> GetAllUsersAsync()
        {
            using var context = await contextFactory.CreateDbContextAsync();
            var users = await context.Users.Include(i=>i.UserRoles)
                                           .ThenInclude(i=>i.Role)
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
            return await context.Roles.FindAsync(id);
        }

        public async Task<IUser?> GetUserByIdAsync(string id)
        {
            using var context = await contextFactory.CreateDbContextAsync();
            return await context.Users.AsNoTracking()
                                      .Include(i=>i.UserRoles)
                                      .ThenInclude(i=>i.Role).FirstOrDefaultAsync(i=>i.Id == id);
        }

        public Task SetPasswordAsync(string id, string Password)
        {
            throw new NotImplementedException();
        }

        public Task SignInAsync(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(IUser user)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserAsync(IUser user)
        {
            using var context = await contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var _user = user as ApplicationUser;
                if (_user == null) throw new ArgumentException("Invalid user type");
                await context.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(u => u.UserName, user.UserName)
                        .SetProperty(u => u.NormalizedUserName, user.UserName?.ToUpper())
                        .SetProperty(u => u.DisplayName, user.DisplayName)
                        .SetProperty(u => u.Email, user.Email)
                        .SetProperty(u => u.NormalizedEmail, user.Email?.ToUpper())
                );

                var currentUserRoles = _user.UserRoles;// user.GetRoles();
                var existingsUserRoles = await context.UserRoles.AsNoTracking().Where(i=>i.UserId == user.Id).ToListAsync();

                var rolesToAdd = currentUserRoles.Where(i=>existingsUserRoles.Any(r=>r.RoleId == i.RoleId) == false)
                                                 .Select(i=> new ApplicationUserRole { RoleId = i.RoleId, UserId = user.Id });
                if (rolesToAdd.Count() > 0)
                {
                    context.UserRoles.AddRange(rolesToAdd);
                }
                
                var rolesToRemove = existingsUserRoles.Where(i=>currentUserRoles.Any(r=>r.RoleId == i.RoleId) == false);
                if (rolesToRemove.Count() > 0)
                    context.UserRoles.RemoveRange(rolesToRemove);
                
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch(Exception ex)
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
                    .SetProperty(u => u.NormalizedName, role.Name?.ToUpper())
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
            var _user = user as ApplicationUser;
            if (_user == null) throw new ArgumentException("Invalid user type");
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(_user);
            await userManager.ResetPasswordAsync(_user, resetToken, newPassword);
        }
    }
}
