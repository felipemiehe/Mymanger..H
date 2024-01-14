using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Auth.Entities
{
    public class UserAdminRolescontrol
    {
        // entidade criada para gerencias roles criadas por seu respectivo Admin
        [Key]
        public int Id { get; set; }

        public string? NameRoleVisual { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RoleId { get; set; }

        public ApplicationUser User { get; set; }
        public ApplicationRoles Role { get; set; }
    }
}
