using System;
using System.ComponentModel.DataAnnotations;

namespace Auth.DTO
{
    public class AtualizaOutroUserDTO

    {
        public AtualizaOutroUserDTO(AtualizaOutroUserDTO model)
        {
            this.Email = model.Email;
            this.PhoneNumber = model.PhoneNumber;
            this.Name = model.Name;
            this.Cpf = model.Cpf;
            this.CodigoUnico = model.Email;
            
        }

        public AtualizaOutroUserDTO()
        {
            
        }

        [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }        

        [Required(ErrorMessage = "PhoneNumber is required")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Cpf is required")]
        public string? Cpf { get; set; }

        [Required(ErrorMessage = "CodigoUnico is required")]
        public string? CodigoUnico { get; set; }


    }


}
