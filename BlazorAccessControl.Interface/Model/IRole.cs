using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAccessControl.Interface
{
    public interface IRole
    {
        public string Id { get; }
        public string? Name { get; set; }

    }
}
