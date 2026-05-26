namespace BlazorAccessControl.Interface
{
    public interface IUser
    {
        public string Id { get; set; }

        public string? UserName { get; set; }

        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public ICollection<IRole> GetRoles();

        public void SetRoles(ICollection<IRole> roles);
    }
}
