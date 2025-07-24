using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models
{
    public class ParametroSistema
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O cabeçalho do sistema é obrigatório")]
        [StringLength(200, ErrorMessage = "O cabeçalho deve ter no máximo 200 caracteres")]
        [Display(Name = "Cabeçalho do Sistema")]
        public string CabecalhoSistema { get; set; } = string.Empty;

        [Required(ErrorMessage = "A versão do sistema é obrigatória")]
        [StringLength(50, ErrorMessage = "A versão deve ter no máximo 50 caracteres")]
        [Display(Name = "Versão do Sistema")]
        public string VersaoSistema { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do rodapé é obrigatório")]
        [StringLength(200, ErrorMessage = "O nome do rodapé deve ter no máximo 200 caracteres")]
        [Display(Name = "Nome do Rodapé")]
        public string NomeRodape { get; set; } = string.Empty;

        [Display(Name = "Cor de Fundo da Tela de Login")]
        [StringLength(20, ErrorMessage = "A cor deve ter no máximo 20 caracteres")]
        public string CorFundoLogin { get; set; } = "#f8f9fa";

        [Display(Name = "Descrição do Cabeçalho da Tela de Login")]
        [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres")]
        public string DescricaoCabecalhoLogin { get; set; } = string.Empty;

        [Display(Name = "Google reCAPTCHA Site Key")]
        [StringLength(200, ErrorMessage = "A chave deve ter no máximo 200 caracteres")]
        public string? ReCaptchaSiteKey { get; set; }

        [Display(Name = "Google reCAPTCHA Secret Key")]
        [StringLength(200, ErrorMessage = "A chave deve ter no máximo 200 caracteres")]
        public string? ReCaptchaSecretKey { get; set; }

        [Display(Name = "Ativar Google reCAPTCHA no Login/Cadastro")]
        public bool ReCaptchaEnabled { get; set; } = false;

        public DateTime DataCriacao { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
} 