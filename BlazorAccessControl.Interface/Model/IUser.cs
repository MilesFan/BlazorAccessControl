namespace BlazorAccessControl.Interface
{
    public interface IUser
    {
        public string Id { get; }

        public string UserName { get;}

        public string DisplayName { get; }

        public string Email { get; }

        public ICollection<IUserRole> UserRoles { get; }
    }
}
