using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using Npgsql;

namespace Dashboard.Controllers
{
    public class AlterarSenhaController : Controller
    {
        private readonly IConfiguration _configuration;

        public AlterarSenhaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!HttpContext.Session.Keys.Contains("UserId"))
                return RedirectToAction("Index", "Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string senhaAtual, string novaSenha, string confirmarNovaSenha)
        {
            if (!HttpContext.Session.Keys.Contains("UserId"))
                return RedirectToAction("Index", "Login");

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                TempData["Erro"] = "Usuário não autenticado.";
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(senhaAtual) || string.IsNullOrEmpty(novaSenha) || string.IsNullOrEmpty(confirmarNovaSenha))
            {
                TempData["Erro"] = "Todos os campos são obrigatórios.";
                return View();
            }

            if (novaSenha != confirmarNovaSenha)
            {
                TempData["Erro"] = "A nova senha e a confirmação não coincidem.";
                return View();
            }

            if (novaSenha.Length < 6)
            {
                TempData["Erro"] = "A nova senha deve ter pelo menos 6 caracteres.";
                return View();
            }

            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString))
            {
                TempData["Erro"] = "Configuração de banco de dados não encontrada.";
                return View();
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT senha FROM usuarios WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    var storedPassword = cmd.ExecuteScalar() as string;
                    if (storedPassword == null)
                    {
                        TempData["Erro"] = "Usuário não encontrado.";
                        return View();
                    }

                    var senhaAtualHash = HashPassword(senhaAtual);
                    if (storedPassword != senhaAtualHash && storedPassword != senhaAtual)
                    {
                        TempData["Erro"] = "Senha atual incorreta.";
                        return View();
                    }
                }

                // Atualizar senha
                using (var cmd = new NpgsqlCommand("UPDATE usuarios SET senha = @senha, data_atualizacao = @dataAlteracao WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@senha", HashPassword(novaSenha));
                    cmd.Parameters.AddWithValue("@dataAlteracao", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Sucesso"] = "Senha alterada com sucesso! Faça login novamente.";
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 