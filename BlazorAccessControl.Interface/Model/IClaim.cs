using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAccessControl.Interface
{
    public interface IClaim<TKey> where TKey:System.IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
    }
}
