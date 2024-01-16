using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace front.Models.Enums
{
    public enum UserRole
    {
        [Display(Name = "Técnico Geral")]
        TecnicoGeral,

        [Display(Name = "Fiscais")]
        Fiscais,

        [Display(Name = "Reporter")]
        Reporter,

        [Display(Name = "Técnico Hidráulica")]
        TecnicoHidraulica,

        [Display(Name = "Técnico Elétrica")]
        TecnicoEletrica,        
    }
}
