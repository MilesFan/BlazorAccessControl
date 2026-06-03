namespace BlazorAccessControl.Interface
{
    public interface IUser
    {
        public string Id { get; set; }

        public string? UserName { get; set; }

        public string? DisplayName { get; }

        public string? Email { get; set; }
    
        public ICollection<IRole> GetRoles();
        public ICollection<IClaim> GetClaims(string? ClaimType = null);
        
        public void SetRoles(ICollection<IRole> roles);
        public void SetClaims(ICollection<IClaim> roles);

        public void UpsertClaim(string ClaimType, string? ClaimValue);

        public void RemoveClaim(string ClaimType);

    }
}
