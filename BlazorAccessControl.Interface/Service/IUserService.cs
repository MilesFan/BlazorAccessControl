using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorAccessControl.Interface
{
    public interface IUserService<TKey> where TKey:System.IEquatable<TKey>
    {
        public IUser<TKey>? CurrentUser { get; }
        public Task<ICollection<IRole<TKey>>> GetAllRolesAsync();
        public Task<ICollection<IUser<TKey>>> GetAllUsersAsync();
        public Task<IUser<TKey>?> GetUserByIdAsync(TKey id);
        public Task<IUser<TKey>?> GetUserByNameAsync(string UserName);
        public Task<ICollection<IRole<TKey>>> GetUserRolesAsync(IUser<TKey> user);
        public Task<IRole<TKey>?> GetRoleByIdAsync(TKey id);
        public Task CreateUserAsync(IUser<TKey> user);
        public Task UpdateUserAsync(IUser<TKey> user);
        public Task CreateRoleAsync(IRole<TKey> role);
        public Task UpdateRoleAsync(IRole<TKey> role);
        public Task SetPasswordAsync(TKey id, string Password);
        public Task DeleteUserByIdAsync(TKey id);
        public Task DeleteRoleByIdAsync(TKey id);
        public Task SignInAsync(string UserName);
        public Task SignInAsync(string UserName, string Password);
        public Task SignInAsync(Uri ExternalUrl);
        public Task SignInWithTokenAsync(string token);
        public Task PasswordSignIn(string UserName, string Password);
        public Task SignOutAsync(IUser<TKey> user);
        public Task SignOutAsync();
        public Task ChangePasswordAsync(IUser<TKey> user, string oldPassword, string newPassword);
        public Task ResetPasswordAsync(IUser<TKey> user, string newPassword);

        public string GetAntiForgeryToken();
        
        public string? GetPasswordLoginEndPoint();
        public string? GetSignOutEndPoint();
        public string? GetOAuthAuthenticationEndPoint();
        public string? GetOAuthValidationEndPoint();
        public TKey NewUserId();
        public TKey NewUserClaimId();
        //public IClaim<TKey> NewUserClaim(string ClaimType, string ClaimValue);
        public TKey NewRoleId();
    }
}
