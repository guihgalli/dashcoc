using Microsoft.AspNetCore.Mvc;
using Dashboard.Models;
using Gerente.Filters;
using Npgsql;

namespace Dashboard.Controllers
{
    public class ParametroSistemaController : BaseController
    {
        public ParametroSistemaController(IConfiguration configuration) : base(configuration)
        {
        }

        public IActionResult Index()
        {
            var parametros = ObterParametrosSistema();
            ViewBag.ParametrosSistema = parametros;
            return View(parametros);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            var parametros = ObterParametrosSistema();
            if (parametros == null)
            {
                // Criar parâmetros padrão se não existirem
                parametros = new ParametroSistema
                {
                    CabecalhoSistema = "DorowCamp",
                    VersaoSistema = "1.0.0",
                    NomeRodape = "Sistema DorowCamp 2025©",
                    CorFundoLogin = "#f8f9fa",
                    DescricaoCabecalhoLogin = ""
                };
            }
            ViewBag.ParametrosSistema = parametros;
            return View(parametros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ParametroSistema parametros)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (parametros.Id == 0)
                    {
                        // Criar novos parâmetros
                        SalvarParametrosSistema(parametros);
                        TempData["Sucesso"] = "Parâmetros do sistema criados com sucesso!";
                    }
                    else
                    {
                        // Atualizar parâmetros existentes
                        AtualizarParametrosSistema(parametros);
                        TempData["Sucesso"] = "Parâmetros do sistema atualizados com sucesso!";
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = "Erro ao salvar parâmetros: " + ex.Message;
                }
            }

            return View(parametros);
        }

        private ParametroSistema? ObterParametrosSistema()
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                return null;
            }

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "SELECT id, cabecalho_sistema, versao_sistema, nome_rodape, cor_fundo_login, descricao_cabecalho_login, recaptcha_site_key, recaptcha_secret_key, recaptcha_enabled, data_criacao, data_atualizacao FROM parametros_sistema ORDER BY id LIMIT 1", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ParametroSistema
                                {
                                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                    CabecalhoSistema = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    VersaoSistema = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    NomeRodape = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    CorFundoLogin = reader.IsDBNull(4) ? "#f8f9fa" : reader.GetString(4),
                                    DescricaoCabecalhoLogin = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    ReCaptchaSiteKey = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    ReCaptchaSecretKey = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    ReCaptchaEnabled = !reader.IsDBNull(8) ? reader.GetBoolean(8) : false,
                                    DataCriacao = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                    DataAlteracao = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Retornar null em caso de erro
            }

            return null;
        }

        private void SalvarParametrosSistema(ParametroSistema parametros)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO parametros_sistema (cabecalho_sistema, versao_sistema, nome_rodape, cor_fundo_login, descricao_cabecalho_login, recaptcha_site_key, recaptcha_secret_key, recaptcha_enabled, data_criacao, data_atualizacao) 
                      VALUES (@cabecalho, @versao, @rodape, @corFundoLogin, @descCabecalhoLogin, @recaptchaSiteKey, @recaptchaSecretKey, @recaptchaEnabled, @dataCriacao, @dataAlteracao)", conn))
                {
                    cmd.Parameters.AddWithValue("@cabecalho", parametros.CabecalhoSistema);
                    cmd.Parameters.AddWithValue("@versao", parametros.VersaoSistema);
                    cmd.Parameters.AddWithValue("@rodape", parametros.NomeRodape);
                    cmd.Parameters.AddWithValue("@corFundoLogin", parametros.CorFundoLogin);
                    cmd.Parameters.AddWithValue("@descCabecalhoLogin", parametros.DescricaoCabecalhoLogin);
                    cmd.Parameters.AddWithValue("@recaptchaSiteKey", (object?)parametros.ReCaptchaSiteKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@recaptchaSecretKey", (object?)parametros.ReCaptchaSecretKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@recaptchaEnabled", parametros.ReCaptchaEnabled);
                    cmd.Parameters.AddWithValue("@dataCriacao", DateTime.Now);
                    cmd.Parameters.AddWithValue("@dataAlteracao", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AtualizarParametrosSistema(ParametroSistema parametros)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE parametros_sistema 
                      SET cabecalho_sistema = @cabecalho, versao_sistema = @versao, nome_rodape = @rodape, cor_fundo_login = @corFundoLogin, descricao_cabecalho_login = @descCabecalhoLogin, recaptcha_site_key = @recaptchaSiteKey, recaptcha_secret_key = @recaptchaSecretKey, recaptcha_enabled = @recaptchaEnabled, data_atualizacao = @dataAlteracao 
                      WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", parametros.Id);
                    cmd.Parameters.AddWithValue("@cabecalho", parametros.CabecalhoSistema);
                    cmd.Parameters.AddWithValue("@versao", parametros.VersaoSistema);
                    cmd.Parameters.AddWithValue("@rodape", parametros.NomeRodape);
                    cmd.Parameters.AddWithValue("@corFundoLogin", parametros.CorFundoLogin);
                    cmd.Parameters.AddWithValue("@descCabecalhoLogin", parametros.DescricaoCabecalhoLogin);
                    cmd.Parameters.AddWithValue("@recaptchaSiteKey", (object?)parametros.ReCaptchaSiteKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@recaptchaSecretKey", (object?)parametros.ReCaptchaSecretKey ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@recaptchaEnabled", parametros.ReCaptchaEnabled);
                    cmd.Parameters.AddWithValue("@dataAlteracao", DateTime.Now);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Parâmetros não encontrados para atualização.");
                    }
                }
            }
        }
    }
} 