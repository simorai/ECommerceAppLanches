using System.Text.RegularExpressions;

namespace AppLanches.Validations
{
    class Validator : IValidator
    {
        public string NomeErro { get; set; } = "";
        public string EmailErro { get; set; } = "";
        public string TelefoneErro { get; set; } = "";
        public string SenhaErro { get; set; } = "";

        private const string NomeVazioErroMsg = "Por favor, informe o seu nome.";
        private const string NomeInvalidoErroMsg = "Por favor, informe um nome válido.";
        private const string EmailVazioErroMsg = "Por favor, informe o seu e-mail.";
        private const string EmailInvalidoErroMsg = "Por favor, informe um e-mail válido.";
        private const string TelefoneVazioErroMsg = "Por favor, informe o seu telefone.";
        private const string TelefoneInvalidoErroMsg = "Por favor, informe um telefone válido.";
        private const string SenhaVaziaErroMsg = "Por favor, informe a sua senha.";
        private const string SenhaInvalidaErroMsg = "A senha deve ter pelo menos 8 caracteres, incluindo letras, números e símbolos especiais.";

        public Task<bool> Validar(string nome, string email, string telefone, string senha)
        {
            var isNomeValido = ValidarNome(nome);
            var isEmailValido = ValidarEmail(email);
            var isTelefoneValido = ValidarTelefone(telefone);
            var isSenhaValida = ValidarSenha(senha);

            return Task.FromResult(isNomeValido && isEmailValido && isTelefoneValido && isSenhaValida);
        }

        private bool ValidarSenha(string senha)
        {
            if (string.IsNullOrEmpty(senha))
            {
                SenhaErro = SenhaVaziaErroMsg;
                return false;
            }
            if (senha.Length < 8 || Regex.IsMatch(senha, @"[a-zA-Z]") == false ||
                Regex.IsMatch(senha, @"[0-9]") == false ||
                Regex.IsMatch(senha, @"[\W_]") == false)
            {
                SenhaErro = SenhaInvalidaErroMsg;
                return false;
            }
            SenhaErro = "";
            return true;
        }

        private bool ValidarTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
            {
                TelefoneErro = TelefoneVazioErroMsg;
                return false;
            }
            if (!Regex.IsMatch(telefone, @"^\(?\d{2}\)? ?9?\d{4}-?\d{4}$"))
            {
                TelefoneErro = TelefoneInvalidoErroMsg;
                return false;
            }
            TelefoneErro = "";
            return true;
        }

        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                EmailErro = EmailVazioErroMsg;
                return false;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailErro = EmailInvalidoErroMsg;
                return false;
            }
            EmailErro = "";
            return true;
        }

        private bool ValidarNome(string nome)
        {
            if (string.IsNullOrEmpty(nome))
            {
                NomeErro = NomeVazioErroMsg;
                return false;
            }

            if (nome.Length < 3 || nome.Length > 50)
            {
                NomeErro = NomeInvalidoErroMsg;
                return false;
            }

            NomeErro = "";
            return true;
        }

    }
}
