﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Entities

{
    public class UserxUser
    {
        // AVISO PARA ESSA ENTIDADE ------!//
        // #### Ela não tem relação com FK então, nao tem o softdelete Cuidado, so lembrete para o futuro
        // Nao tem problema ao deletar User porque ele só inativo no banco de dados

        [Key]
        public int Id { get; set; }

        [Required]
        public string? User_Admin_Id { get; set; }

        [Required]
        public string? User_Agregado_Id { get; set; }

        public DateTime Data_criacao { get; set; } = DateTime.Now;
        public string? User_Admin_agregado { get; set; }

        // Propriedades de navegação
        [ForeignKey("User_Admin_Id")]
        public ApplicationUser? UserAdmin { get; set; }

        [ForeignKey("User_Agregado_Id")]
        public ApplicationUser? UserAgregado { get; set; }


    }
}