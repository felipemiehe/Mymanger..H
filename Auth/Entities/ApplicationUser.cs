using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities
{
    public class ApplicationUser : IdentityUser
    {
                        
        public string? Name { get; set; }

        [Required]
        public string Cpf { get; set; }

        [Required]        
        public string CodigoUnico { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<UserAdminRolescontrol> UserAdminRolescontrols { get; set; }        


    }
}
