using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorAccessControl.Interface
{
    public interface IUserService
    {
        public IUser? CurrentUser { get; }
        public Task<ICollection<IRole>> GetAllRolesAsync();
        public Task<ICollection<IUser>> GetAllUsersAsync();
        public Task<IUser?> GetUserByIdAsync(string id);
        public Task<IUser?> GetUserByNameAsync(string id);
        public Task<ICollection<IRole>> GetUserRolesAsync(IUser user);
        public Task<IRole?> GetRoleByIdAsync(string id);
        public Task CreateUserAsync(IUser user);
        public Task UpdateUserAsync(IUser user);
        public Task CreateRoleAsync(IRole role);
        public Task UpdateRoleAsync(IRole role);
        public Task SetPasswordAsync(string id, string Password);
        public Task DeleteUserByIdAsync(string id);
        public Task DeleteRoleByIdAsync(string id);
        public Task SignInAsync(string UserName);
        public Task SignInAsync(string UserName, string Password);
        public Task SignInAsync(Uri ExternalUrl);
        public Task SignInWithTokenAsync(string token);
        public Task PasswordSignIn(string UserName, string Password);
        public Task SignOutAsync(IUser user);
        public Task SignOutAsync();
        public Task ChangePasswordAsync(IUser user, string oldPassword, string newPassword);
        public Task ResetPasswordAsync(IUser user, string newPassword);

        public string GetAntiForgeryToken();
        
        public string? GetPasswordLoginEndPoint();
        public string? GetSignOutEndPoint();
        public string? GetOAuthAuthenticationEndPoint();
        public string? GetOAuthValidationEndPoint();
    }
}
