namespace Auth.Entities
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string TecnicoGeral = "Tecnico Geral";
        public const string Fiscais = "fiscais";
        public const string Reporter = "reporter";
        public const string TecnicoHidraulica = "Tecnico hidraulica";
        public const string TecnicoEletrica = "Tecnico eletrica";

        public static bool ContainsRole(string inputString)
        {
            return Admin.Contains(inputString) ||
                   User.Contains(inputString) ||
                   TecnicoGeral.Contains(inputString) ||
                   Fiscais.Contains(inputString) ||
                   Reporter.Contains(inputString) ||
                   TecnicoHidraulica.Contains(inputString) ||
                   TecnicoEletrica.Contains(inputString);
        }

    }
}
