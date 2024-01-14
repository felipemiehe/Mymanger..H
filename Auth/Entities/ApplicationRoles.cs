using Microsoft.AspNetCore.Identity;

namespace Auth.Entities
{
    public class ApplicationRoles: IdentityRole
    {

        public ApplicationRoles() : base()
        {
            
        }
        public ApplicationRoles(string roleName) : base(roleName)
        {
        }

        public ICollection<UserAdminRolescontrol> UserAdminRolescontrols { get; set; }


    }
}
