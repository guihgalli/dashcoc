using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Dashboard.Services;
using Dashboard.Models;
using System.Security.Cryptography;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dashboard.Controllers
{
    public class LoginController : BaseController
    {
        private readonly PasswordResetService _passwordResetService;
        private readonly EmailService _emailService;

        public LoginController(IConfiguration configuration, PasswordResetService passwordResetService, EmailService emailService) : base(configuration)
        {
            _passwordResetService = passwordResetService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var parametros = ObterParametrosSistema();
            ViewBag.ParametrosSistema = parametros;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string login_user_x, string i_x2, bool lembrarSenha = false, string? g_recaptcha_response = null)
        {
            Console.WriteLine("[DEBUG] Entrou no método Index POST do LoginController");
            var email = login_user_x?.Trim();
            var password = i_x2?.Trim();
            
            // Decodificar a senha Base64 se não estiver vazia
            if (!string.IsNullOrEmpty(password))
            {
                try
                {
                    var passwordBytes = System.Convert.FromBase64String(password);
                    password = System.Text.Encoding.UTF8.GetString(passwordBytes);
                    Console.WriteLine("[DEBUG] Senha decodificada com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Erro ao decodificar senha Base64: {ex.Message}");
                    // Se não conseguir decodificar, usar a senha como está (para compatibilidade)
                }
            }
            var parametros = ObterParametrosSistema();
            ViewBag.ParametrosSistema = parametros;
            Console.WriteLine($"[DEBUG] parametros?.ReCaptchaEnabled: {parametros?.ReCaptchaEnabled}");
            Console.WriteLine($"[DEBUG] g_recaptcha_response: {g_recaptcha_response}");
            if (parametros?.ReCaptchaEnabled == true)
            {
                // Validação reCAPTCHA
                if (string.IsNullOrEmpty(g_recaptcha_response) || !ValidateReCaptcha(g_recaptcha_response, "login").GetAwaiter().GetResult())
                {
                    ViewBag.Error = "Falha na validação do reCAPTCHA. Tente novamente.";
                    return View();
                }
            }
            // Verificar se os parâmetros não são null
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "E-mail e senha são obrigatórios.";
                return View();
            }
            
            Console.WriteLine($"[DEBUG] Email: {email}");
            Console.WriteLine($"[DEBUG] Senha (após decodificação): {password}");

            // Verificar se é uma requisição AJAX
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                         Request.Headers["Content-Type"] == "application/json";

            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                
                // Buscar o usuário pelo e-mail
                using (var cmd = new NpgsqlCommand("SELECT id, email, nome, senha, ativo FROM usuarios WHERE email = @email", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var userId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            var userEmail = reader.GetString(1);
                            var userName = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            var storedPassword = reader.GetString(3);
                            var userAtivo = reader.IsDBNull(4) ? false : reader.GetBoolean(4);
                            var hashedPassword = HashPassword(password);
                            bool passwordValid = false;
                            
                            // Tentar comparar com hash primeiro
                            if (storedPassword == hashedPassword)
                            {
                                passwordValid = true;
                            }
                            // Se não funcionar, tentar com texto plano (para usuários antigos)
                            else if (storedPassword == password)
                            {
                                passwordValid = true;
                                // Atualizar para hash na próxima vez
                                if (userId > 0)
                                {
                                    UpdatePasswordToHash(conn, userId, hashedPassword);
                                }
                            }
                            else
                            {
                            }
                            
                            if (!userAtivo)
                            {
                                if (isAjax)
                                {
                                    return Json(new { success = false, message = "Usuário inativo. Entre em contato com o administrador." });
                                }
                                ViewBag.Error = "Usuário inativo. Entre em contato com o administrador.";
                                return View();
                            }
                            
                            if (passwordValid)
                            {
                                // Obter informações do perfil de acesso
                                PerfilAcesso? perfilAcesso = null;
                                if (userId > 0)
                                {
                                    perfilAcesso = ObterPerfilAcessoUsuario(userId);
                                }
                                
                                HttpContext.Session.SetInt32("UserId", userId);
                                HttpContext.Session.SetString("Username", userEmail);
                                HttpContext.Session.SetString("UserName", userName);
                                HttpContext.Session.SetString("UserProfile", perfilAcesso?.Nome ?? "Sem perfil");
                                
                                if (isAjax)
                                {
                                    return Json(new { success = true, message = "Login realizado com sucesso!" });
                                }
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                        }
                    }
                }
                
                // Se chegou aqui, o usuário não foi encontrado
                
                if (isAjax)
                {
                    return Json(new { success = false, message = "E-mail ou senha inválidos." });
                }
                
                ViewBag.Error = "E-mail ou senha inválidos.";
                return View();
            }
        }

        private void UpdatePasswordToHash(NpgsqlConnection conn, int userId, string hashedPassword)
        {
            if (userId <= 0 || string.IsNullOrEmpty(hashedPassword))
            {
                return;
            }
            
            try
            {
                using (var cmd = new NpgsqlCommand("UPDATE usuarios SET senha = @senha WHERE id = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@senha", hashedPassword);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Silenciosamente falhar se não conseguir atualizar
            }
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }
            
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    return System.Convert.ToBase64String(hashedBytes);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return Json(new { success = false, message = "Por favor, informe um e-mail válido." });
            }

            try
            {
                // Verificar se o e-mail existe no cadastro de usuários
                if (!EmailExisteNoCadastro(request.Email))
                {
                    return Json(new { success = false, message = "E-mail não encontrado no cadastro de usuários do sistema." });
                }

                // Gerar token e salvar no banco
                var result = await _passwordResetService.RequestPasswordResetAsync(request.Email);

                // Enviar e-mail de redefinição explicitamente usando o _emailService
                if (result)
                {
                    // Exemplo: se existir um método para envio de e-mail de redefinição
                    // await _emailService.EnviarEmailRedefinicaoSenhaAsync(request.Email);
                    // Se o PasswordResetService já envia, apenas retorne sucesso
                    return Json(new { success = true, message = "E-mail de redefinição enviado com sucesso! Verifique sua caixa de entrada." });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao enviar e-mail de redefinição. Tente novamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao processar solicitação: " + ex.Message });
            }
        }

        private bool EmailExisteNoCadastro(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE email = @email", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private PerfilAcesso? ObterPerfilAcessoUsuario(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }
            
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                return null;
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"SELECT pa.id, pa.nome, pa.descricao, pa.acesso_configuracoes, 
                             pa.acesso_usuarios, pa.acesso_projetos, pa.acesso_relatorios, 
                             pa.acesso_total, pa.ativo
                      FROM usuarios u 
                      LEFT JOIN perfis_acesso pa ON u.perfil_acesso_id = pa.id 
                      WHERE u.id = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PerfilAcesso
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nome = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                AcessoConfiguracoes = reader.IsDBNull(3) ? false : reader.GetBoolean(3),
                                AcessoUsuarios = reader.IsDBNull(4) ? false : reader.GetBoolean(4),
                                AcessoProjetos = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                                AcessoRelatorios = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                                AcessoTotal = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                                Ativo = reader.IsDBNull(8) ? false : reader.GetBoolean(8)
                            };
                        }
                    }
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Token inválido.";
                return View("Index");
            }

            var isValid = await _passwordResetService.ValidateTokenAsync(token);
            if (!isValid)
            {
                ViewBag.Error = "Token inválido ou expirado.";
                return View("Index");
            }

            var parametros = ObterParametrosSistema();
            ViewBag.ParametrosSistema = parametros;
            ViewBag.Token = token;
            return View();
        }

        [HttpGet]
        public IActionResult TestForgotPassword()
        {
            return Json(new { 
                success = true, 
                message = "Endpoint de teste funcionando!",
                timestamp = DateTime.Now,
                serviceRegistered = _passwordResetService != null
            });
        }

        [HttpGet]
        public IActionResult TestLogin(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "Email e senha são obrigatórios" });
                }

                string? connString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connString))
                {
                    return Json(new { success = false, message = "Connection string não encontrada" });
                }

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    
                    // Verificar se o usuário existe
                    using (var cmd = new NpgsqlCommand("SELECT id, email, nome, senha, ativo FROM usuarios WHERE email = @email", conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var userId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                var userEmail = reader.GetString(1);
                                var userName = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                var storedPassword = reader.GetString(3);
                                var userAtivo = reader.IsDBNull(4) ? false : reader.GetBoolean(4);
                                
                                var hashedPassword = HashPassword(password);
                                
                                return Json(new { 
                                    success = true, 
                                    message = "Usuário encontrado",
                                    data = new {
                                        userId = userId,
                                        email = userEmail,
                                        name = userName,
                                        active = userAtivo,
                                        storedPassword = storedPassword,
                                        inputPassword = password,
                                        hashedPassword = hashedPassword,
                                        hashMatch = storedPassword == hashedPassword,
                                        plainMatch = storedPassword == password
                                    }
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Usuário não encontrado" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetConfirm request)
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return Json(new { success = false, message = "Dados inválidos." });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return Json(new { success = false, message = "As senhas não coincidem." });
            }

            if (request.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "A senha deve ter pelo menos 6 caracteres." });
            }

            try
            {
                var result = await _passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword);
                if (result)
                {
                    return Json(new { success = true, message = "Senha alterada com sucesso! Você pode fazer login com sua nova senha." });
                }
                else
                {
                    return Json(new { success = false, message = "Token inválido ou expirado." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao alterar senha. Tente novamente." });
            }
        }

        [HttpGet]
        public IActionResult CriarConta()
        {
            Console.WriteLine("=== DEBUG: CriarConta GET ===");
            var parametros = ObterParametrosSistema();
            Console.WriteLine($"parametros obtidos: {parametros != null}");
            if (parametros != null)
            {
                Console.WriteLine($"ReCaptchaEnabled: {parametros.ReCaptchaEnabled}");
                Console.WriteLine($"ReCaptchaSiteKey: {parametros.ReCaptchaSiteKey}");
            }
            ViewBag.ParametrosSistema = parametros;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarConta(CriarContaViewModel model, string? g_recaptcha_response)
        {
            Console.WriteLine("=== DEBUG: CriarConta POST ===");
            Console.WriteLine($"Model recebido: Nome='{model?.Nome}', Email='{model?.Email}'");
            Console.WriteLine($"g_recaptcha_response: {g_recaptcha_response}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState Errors ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }
            
            var parametros = ObterParametrosSistema();
            ViewBag.ParametrosSistema = parametros;
            Console.WriteLine($"ReCaptchaEnabled: {parametros?.ReCaptchaEnabled}");
            
            if (parametros?.ReCaptchaEnabled == true)
            {
                Console.WriteLine("Validando reCAPTCHA...");
                // Validação reCAPTCHA
                if (string.IsNullOrEmpty(g_recaptcha_response) || !await ValidateReCaptcha(g_recaptcha_response, "register"))
                {
                    Console.WriteLine("Falha na validação do reCAPTCHA");
                    ModelState.AddModelError("", "Falha na validação do reCAPTCHA. Tente novamente.");
                    return View(model);
                }
                Console.WriteLine("reCAPTCHA validado com sucesso");
            }
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState inválido, retornando view");
                return View(model);
            }

            try
            {
                Console.WriteLine("Verificando se e-mail já existe...");
                // Verificar se o e-mail já existe
                if (EmailExisteNoCadastro(model.Email))
                {
                    Console.WriteLine("E-mail já existe no cadastro");
                    ModelState.AddModelError("Email", "E-mail já cadastrado no sistema.");
                    return View(model);
                }

                Console.WriteLine("Gerando senha temporária...");
                // Gerar senha temporária
                var senhaTemporaria = GerarSenhaTemporaria();
                var senhaHash = HashPassword(senhaTemporaria);

                Console.WriteLine("Salvando usuário temporário...");
                // Salvar usuário com status inativo
                var userId = await SalvarUsuarioTemporario(model, senhaHash);
                if (userId == null)
                {
                    Console.WriteLine("Erro ao salvar usuário temporário");
                    ModelState.AddModelError("", "Erro ao cadastrar usuário. Tente novamente.");
                    return View(model);
                }
                Console.WriteLine($"Usuário salvo com ID: {userId}");

                Console.WriteLine("Enviando e-mail de confirmação...");
                // Enviar e-mail de confirmação para o usuário
                await _emailService.EnviarEmailConfirmacaoCadastroAsync(model.Email, model.Nome);

                Console.WriteLine("Enviando e-mail de notificação para admin...");
                // Enviar e-mail de notificação para o administrador
                await _emailService.EnviarEmailNotificacaoAdminAsync(model.Email, model.Nome);

                Console.WriteLine("Conta criada com sucesso!");
                TempData["Sucesso"] = "Conta criada com sucesso! Você receberá um e-mail de confirmação e um administrador será notificado para ativar sua conta.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar conta: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Erro ao criar conta: {ex.Message}");
                return View(model);
            }
        }

        private string GerarSenhaTemporaria()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<int?> SalvarUsuarioTemporario(CriarContaViewModel model, string senhaHash)
        {
            try
            {
                string? connString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connString))
                {
                    return null;
                }

                using (var conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();
                    
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO usuarios (nome, email, senha, ativo, data_criacao, data_atualizacao) VALUES (@nome, @email, @senha, @ativo, @dataCriacao, @dataAlteracao) RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("@nome", model.Nome);
                        cmd.Parameters.AddWithValue("@email", model.Email);
                        cmd.Parameters.AddWithValue("@senha", senhaHash);
                        cmd.Parameters.AddWithValue("@ativo", false); // Usuário inativo por padrão
                        cmd.Parameters.AddWithValue("@dataCriacao", DateTime.Now);
                        cmd.Parameters.AddWithValue("@dataAlteracao", DateTime.Now);
                        
                        var result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private ParametroSistema? ObterParametrosSistema()
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString))
                return null;
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "SELECT id, cabecalho_sistema, versao_sistema, nome_rodape, cor_fundo_login, cor_fundo_sistema, descricao_cabecalho_login, recaptcha_site_key, recaptcha_secret_key, recaptcha_enabled, data_criacao, data_atualizacao FROM parametros_sistema ORDER BY id LIMIT 1", conn))
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
                                    DescricaoCabecalhoLogin = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    ReCaptchaSiteKey = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    ReCaptchaSecretKey = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    ReCaptchaEnabled = !reader.IsDBNull(9) && reader.GetBoolean(9),
                                    DataCriacao = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10),
                                    DataAlteracao = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11)
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

        private async Task<bool> ValidateReCaptcha(string token, string action)
        {
            Console.WriteLine("[reCAPTCHA] Início da validação do reCAPTCHA.");
            Console.WriteLine($"[reCAPTCHA] Token recebido do cliente: {token}");
            var parametros = ObterParametrosSistema();
            var secret = parametros?.ReCaptchaSecretKey;
            Console.WriteLine($"[reCAPTCHA] Valor de secret: {secret}");
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[reCAPTCHA] Secret ou token ausente. Secret: {secret}, Token: {token}");
                return false;
            }
            using (var client = new HttpClient())
            {
                var endpoint = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}";
                Console.WriteLine($"[reCAPTCHA] Endpoint gerado para requisição: {endpoint}");
                var response = await client.PostAsync(endpoint, null);
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[reCAPTCHA] Resposta recebida do Google (JSON): {json}");
                var result = JObject.Parse(json);
                bool sucesso = result.Value<bool>("success") && result.Value<string>("action") == action && result.Value<double>("score") > 0.5;
                Console.WriteLine($"[reCAPTCHA] Resultado final da validação: {(sucesso ? "SUCESSO" : "FALHA")}");
                if (!sucesso)
                {
                    Console.WriteLine($"[reCAPTCHA] Motivo da falha: success={result.Value<bool>("success")}, action={result.Value<string>("action")}, score={result.Value<double>("score")}");
                }
                return sucesso;
            }
        }
    }
} 