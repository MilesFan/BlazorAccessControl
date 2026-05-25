using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAccessControl.Interface
{
    public interface IUserRole
    {
        public IUser User { get; set; }

        public IRole Role { get; set; }
    }
}
