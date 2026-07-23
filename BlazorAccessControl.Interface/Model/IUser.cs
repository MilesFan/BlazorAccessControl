namespace BlazorAccessControl.Interface
{
    public interface IUser<TKey> where TKey:System.IEquatable<TKey>
    {
        public TKey Id { get; set; }

        public string? UserName { get; set; }

        public string? DisplayName { get; }

        public string? Email { get; set; }
    
        public ICollection<IRole<TKey>> GetRoles();
        public ICollection<IClaim<TKey>> GetClaims(string? ClaimType = null);
        
        public void SetRoles(ICollection<IRole<TKey>> roles);
        public void SetClaims(ICollection<IClaim<TKey>> claims);

        public void UpsertClaim(TKey Id, string ClaimType, string? ClaimValue);

        public void RemoveClaim(string ClaimType);
    }
}
