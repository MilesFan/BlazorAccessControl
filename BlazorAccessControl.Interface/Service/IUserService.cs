using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorAccessControl.Interface
{
    public interface IUserService
    {
        public IUser? CurrentUser { get; }
    }
}
