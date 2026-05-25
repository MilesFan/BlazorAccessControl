using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorAccessControl.Interface
{
    public interface IUserService
    {
        public IUser? CurrentUser { get; }

        public Task<ICollection<IUser>> GetAllUsersAsync();

        public Task<IUser?> GetUserByIdAsync(string id);

        public Task AddUser();

        public Task UpdateUser(IUser user);

        public Task DeleteUserById(string id);
        
        public Task SignIn(IUser user);
        public Task SignOut(IUser user);
    }
}
