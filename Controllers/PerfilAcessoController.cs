using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dashboard.Models;
using System.Collections.Generic;
using Gerente.Filters;
using System.Linq; // Added for SelectMany

namespace Dashboard.Controllers
{
    [RequireConfigurationsAccess]
    public class PerfilAcessoController : Controller
    {
        private readonly IConfiguration _configuration;

        public PerfilAcessoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var perfis = ObterTodosPerfis();
            return View(perfis);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PerfilAcessoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PerfilAcessoViewModel perfilViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se o nome já existe
                    if (NomeExiste(perfilViewModel.Nome))
                    {
                        ModelState.AddModelError("Nome", "Já existe um perfil com este nome.");
                        return View(perfilViewModel);
                    }

                    // Salvar perfil
                    SalvarPerfil(perfilViewModel);

                    TempData["Sucesso"] = "Perfil de acesso criado com sucesso!";
                    return RedirectToAction("Index");
                }
                catch (Npgsql.PostgresException ex)
                {
                    string mensagemErro;
                    switch (ex.SqlState)
                    {
                        case "23503": // Foreign key violation
                            mensagemErro = "Erro de integridade referencial no banco de dados.";
                            break;
                        case "23505": // Unique violation
                            mensagemErro = "Já existe um perfil com este nome.";
                            break;
                        case "23514": // Check violation
                            mensagemErro = "Dados inválidos para o perfil.";
                            break;
                        default:
                            mensagemErro = $"Erro no banco de dados: {ex.Message}";
                            break;
                    }
                    TempData["Erro"] = mensagemErro;
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Erro"] = ex.Message;
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = "Erro ao cadastrar perfil. Tente novamente.";
                }
            }

            return View(perfilViewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var perfil = ObterPerfilPorId(id);
            if (perfil == null)
            {
                TempData["Erro"] = "Perfil não encontrado.";
                return RedirectToAction("Index");
            }

            var perfilViewModel = new PerfilAcessoViewModel
            {
                Id = perfil.Id,
                Nome = perfil.Nome,
                Descricao = perfil.Descricao,
                AcessoConfiguracoes = perfil.AcessoConfiguracoes,
                AcessoUsuarios = perfil.AcessoUsuarios,
                AcessoProjetos = perfil.AcessoProjetos,
                AcessoBacklogArquitetura = perfil.AcessoBacklogArquitetura,
                AcessoRelatorios = perfil.AcessoRelatorios,
                AcessoTotal = perfil.AcessoTotal,
                Ativo = perfil.Ativo
            };

            return View(perfilViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PerfilAcessoViewModel perfilViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se o nome já existe (exceto para o perfil atual)
                    if (NomeExiste(perfilViewModel.Nome, perfilViewModel.Id))
                    {
                        ModelState.AddModelError("Nome", "Já existe um perfil com este nome.");
                        return View(perfilViewModel);
                    }

                    // Atualizar perfil
                    AtualizarPerfil(perfilViewModel);

                    TempData["Sucesso"] = "Perfil de acesso atualizado com sucesso!";
                    return RedirectToAction("Index");
                }
                catch (Npgsql.PostgresException ex)
                {
                    string mensagemErro;
                    switch (ex.SqlState)
                    {
                        case "23503": // Foreign key violation
                            mensagemErro = "Erro de integridade referencial no banco de dados.";
                            break;
                        case "23505": // Unique violation
                            mensagemErro = "Já existe um perfil com este nome.";
                            break;
                        case "23514": // Check violation
                            mensagemErro = "Dados inválidos para o perfil.";
                            break;
                        default:
                            mensagemErro = $"Erro no banco de dados: {ex.Message}";
                            break;
                    }
                    TempData["Erro"] = mensagemErro;
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Erro"] = ex.Message;
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = "Erro ao atualizar perfil. Tente novamente.";
                }
            }

            return View(perfilViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                // Verificar se o perfil existe
                var perfil = ObterPerfilPorId(id);
                if (perfil == null)
                {
                    return Json(new { success = false, message = "Perfil não encontrado." });
                }

                // Verificar se não é um perfil padrão
                if (perfil.Nome == "Administrador" || perfil.Nome == "Usuário")
                {
                    return Json(new { success = false, message = "Não é possível excluir perfis padrão do sistema." });
                }

                // Verificar se há usuários usando este perfil
                if (PerfilEmUso(id))
                {
                    return Json(new { success = false, message = "Não é possível excluir este perfil pois há usuários associados a ele." });
                }

                ExcluirPerfil(id);
                return Json(new { success = true, message = "Perfil excluído com sucesso!" });
            }
            catch (Npgsql.PostgresException ex)
            {
                string mensagemErro;
                switch (ex.SqlState)
                {
                    case "23503": // Foreign key violation
                        mensagemErro = "Não é possível excluir este perfil pois ele está sendo utilizado por usuários.";
                        break;
                    case "23505": // Unique violation
                        mensagemErro = "Erro de duplicação de dados.";
                        break;
                    default:
                        mensagemErro = $"Erro no banco de dados: {ex.Message}";
                        break;
                }
                return Json(new { success = false, message = mensagemErro });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao excluir perfil. Tente novamente." });
            }
        }

        [HttpGet]
        public IActionResult TestEstruturaTabela()
        {
            try
            {
                string? connString = _configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(connString))
                {
                    return Json(new { success = false, message = "Connection string não encontrada" });
                }

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    
                    var resultado = new
                    {
                        colunas = new List<object>(),
                        constraints = new List<object>()
                    };
                    
                    // Verificar colunas
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT column_name, data_type, is_nullable, column_default, character_maximum_length
                        FROM information_schema.columns 
                        WHERE table_name = 'perfis_acesso' 
                        ORDER BY ordinal_position", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultado.colunas.Add(new
                                {
                                    nome = reader.GetString(0),
                                    tipo = reader.GetString(1),
                                    nullable = reader.GetString(2),
                                    defaultValue = reader.IsDBNull(3) ? "NULL" : reader.GetString(3),
                                    maxLength = reader.IsDBNull(4) ? "NULL" : reader.GetInt32(4).ToString()
                                });
                            }
                        }
                    }
                    
                    // Verificar constraints
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT conname, contype, pg_get_constraintdef(oid) as definition
                        FROM pg_constraint 
                        WHERE conrelid = 'perfis_acesso'::regclass", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultado.constraints.Add(new
                                {
                                    nome = reader.GetString(0),
                                    tipo = reader.GetString(1),
                                    definicao = reader.GetString(2)
                                });
                            }
                        }
                    }
                    
                    return Json(new { success = true, data = resultado });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public IActionResult TestConstraint()
        {
            try
            {
                string? connString = _configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(connString))
                {
                    return Json(new { success = false, message = "Connection string não encontrada" });
                }

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    
                    // Verificar a constraint específica
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT conname, contype, pg_get_constraintdef(oid) as definition
                        FROM pg_constraint 
                        WHERE conrelid = 'perfis_acesso'::regclass 
                        AND conname = 'chk_perfil_acesso_total'", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Json(new { 
                                    success = true, 
                                    constraint = new
                                    {
                                        nome = reader.GetString(0),
                                        tipo = reader.GetString(1),
                                        definicao = reader.GetString(2)
                                    }
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Constraint chk_perfil_acesso_total não encontrada" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public IActionResult ListarTodasConstraints()
        {
            try
            {
                string? connString = _configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(connString))
                {
                    return Json(new { success = false, message = "Connection string não encontrada" });
                }

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    
                    var constraints = new List<object>();
                    
                    // Listar todas as constraints da tabela
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT conname, contype, pg_get_constraintdef(oid) as definition
                        FROM pg_constraint 
                        WHERE conrelid = 'perfis_acesso'::regclass
                        ORDER BY conname", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                constraints.Add(new
                                {
                                    nome = reader.GetString(0),
                                    tipo = reader.GetString(1),
                                    definicao = reader.GetString(2)
                                });
                            }
                        }
                    }
                    
                    return Json(new { success = true, constraints = constraints });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        private List<PerfilAcesso> ObterTodosPerfis()
        {
            var perfis = new List<PerfilAcesso>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT id, nome, descricao, acesso_configuracoes, acesso_usuarios, acesso_projetos, acesso_backlog_arquitetura, acesso_relatorios, acesso_total, ativo, data_criacao, data_atualizacao FROM perfis_acesso ORDER BY nome", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            perfis.Add(new PerfilAcesso
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nome = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                AcessoConfiguracoes = reader.IsDBNull(3) ? false : reader.GetBoolean(3),
                                AcessoUsuarios = reader.IsDBNull(4) ? false : reader.GetBoolean(4),
                                AcessoProjetos = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                                AcessoBacklogArquitetura = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                                AcessoRelatorios = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                                AcessoTotal = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                                Ativo = reader.IsDBNull(9) ? false : reader.GetBoolean(9),
                                DataCriacao = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10),
                                DataAlteracao = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11)
                            });
                        }
                    }
                }
            }

            return perfis;
        }

        private PerfilAcesso? ObterPerfilPorId(int id)
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
                    "SELECT id, nome, descricao, acesso_configuracoes, acesso_usuarios, acesso_projetos, acesso_backlog_arquitetura, acesso_relatorios, acesso_total, ativo, data_criacao, data_atualizacao FROM perfis_acesso WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
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
                                AcessoBacklogArquitetura = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                                AcessoRelatorios = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                                AcessoTotal = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                                Ativo = reader.IsDBNull(9) ? false : reader.GetBoolean(9),
                                DataCriacao = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10),
                                DataAlteracao = reader.IsDBNull(11) ? DateTime.MinValue : reader.GetDateTime(11)
                            };
                        }
                    }
                }
            }

            return null;
        }

        private bool NomeExiste(string nome, int? idExcluir = null)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM perfis_acesso WHERE nome = @nome";
                if (idExcluir.HasValue)
                {
                    sql += " AND id != @id";
                }

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", nome);
                    if (idExcluir.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@id", idExcluir.Value);
                    }
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool PerfilEmUso(int perfilId)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM usuarios WHERE perfil_acesso_id = @perfilId", conn))
                {
                    cmd.Parameters.AddWithValue("@perfilId", perfilId);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void SalvarPerfil(PerfilAcessoViewModel perfilViewModel)
        {
            // Se Acesso Total é true, todos os outros acessos devem ser true
            bool acessoConfiguracoes = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoConfiguracoes;
            bool acessoUsuarios = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoUsuarios;
            bool acessoProjetos = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoProjetos;
            bool acessoBacklogArquitetura = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoBacklogArquitetura;
            bool acessoRelatorios = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoRelatorios;
            bool acessoParametrosSistema = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoConfiguracoes;

            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                
                // Verificar se a coluna acesso_backlog_arquitetura existe
                bool colunaBacklogExiste = false;
                using (var cmdCheck = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'perfis_acesso' AND column_name = 'acesso_backlog_arquitetura'", conn))
                {
                    colunaBacklogExiste = Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0;
                }
                
                // Verificar se a coluna acesso_parametros_sistema existe
                bool colunaParametrosExiste = false;
                using (var cmdCheckParametros = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'perfis_acesso' AND column_name = 'acesso_parametros_sistema'", conn))
                {
                    colunaParametrosExiste = Convert.ToInt32(cmdCheckParametros.ExecuteScalar()) > 0;
                }
                
                string sqlInsert;
                if (colunaBacklogExiste && colunaParametrosExiste)
                {
                    sqlInsert = @"INSERT INTO perfis_acesso (nome, descricao, acesso_configuracoes, acesso_usuarios, 
                                     acesso_projetos, acesso_backlog_arquitetura, acesso_relatorios, acesso_parametros_sistema, 
                                     acesso_total, ativo) 
                                 VALUES (@nome, @descricao, @acessoConfiguracoes, @acessoUsuarios, @acessoProjetos, 
                                     @acessoBacklogArquitetura, @acessoRelatorios, @acessoParametrosSistema, @acessoTotal, @ativo)";
                }
                else if (colunaBacklogExiste)
                {
                    sqlInsert = @"INSERT INTO perfis_acesso (nome, descricao, acesso_configuracoes, acesso_usuarios, 
                                     acesso_projetos, acesso_backlog_arquitetura, acesso_relatorios, acesso_total, ativo) 
                                 VALUES (@nome, @descricao, @acessoConfiguracoes, @acessoUsuarios, @acessoProjetos, 
                                     @acessoBacklogArquitetura, @acessoRelatorios, @acessoTotal, @ativo)";
                }
                else if (colunaParametrosExiste)
                {
                    sqlInsert = @"INSERT INTO perfis_acesso (nome, descricao, acesso_configuracoes, acesso_usuarios, 
                                     acesso_projetos, acesso_relatorios, acesso_parametros_sistema, acesso_total, ativo) 
                                 VALUES (@nome, @descricao, @acessoConfiguracoes, @acessoUsuarios, @acessoProjetos, 
                                     @acessoRelatorios, @acessoParametrosSistema, @acessoTotal, @ativo)";
                }
                else
                {
                    sqlInsert = @"INSERT INTO perfis_acesso (nome, descricao, acesso_configuracoes, acesso_usuarios, 
                                     acesso_projetos, acesso_relatorios, acesso_total, ativo) 
                                 VALUES (@nome, @descricao, @acessoConfiguracoes, @acessoUsuarios, @acessoProjetos, 
                                     @acessoRelatorios, @acessoTotal, @ativo)";
                }
                
                using (var cmd = new NpgsqlCommand(sqlInsert, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", perfilViewModel.Nome);
                    cmd.Parameters.AddWithValue("@descricao", perfilViewModel.Descricao ?? "");
                    cmd.Parameters.AddWithValue("@acessoConfiguracoes", acessoConfiguracoes);
                    cmd.Parameters.AddWithValue("@acessoUsuarios", acessoUsuarios);
                    cmd.Parameters.AddWithValue("@acessoProjetos", acessoProjetos);
                    if (colunaBacklogExiste)
                    {
                        cmd.Parameters.AddWithValue("@acessoBacklogArquitetura", acessoBacklogArquitetura);
                    }
                    cmd.Parameters.AddWithValue("@acessoRelatorios", acessoRelatorios);
                    if (colunaParametrosExiste)
                    {
                        cmd.Parameters.AddWithValue("@acessoParametrosSistema", acessoParametrosSistema);
                    }
                    cmd.Parameters.AddWithValue("@acessoTotal", perfilViewModel.AcessoTotal);
                    cmd.Parameters.AddWithValue("@ativo", perfilViewModel.Ativo);
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AtualizarPerfil(PerfilAcessoViewModel perfilViewModel)
        {
            // Se Acesso Total é true, todos os outros acessos devem ser true
            bool acessoConfiguracoes = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoConfiguracoes;
            bool acessoUsuarios = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoUsuarios;
            bool acessoProjetos = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoProjetos;
            bool acessoBacklogArquitetura = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoBacklogArquitetura;
            bool acessoRelatorios = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoRelatorios;
            bool acessoParametrosSistema = perfilViewModel.AcessoTotal ? true : perfilViewModel.AcessoConfiguracoes;

            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                
                // Verificar se a coluna acesso_backlog_arquitetura existe
                bool colunaExiste = false;
                using (var cmdCheck = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'perfis_acesso' AND column_name = 'acesso_backlog_arquitetura'", conn))
                {
                    colunaExiste = Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0;
                }
                
                // Verificar se a coluna acesso_parametros_sistema existe
                bool colunaParametrosExiste = false;
                using (var cmdCheckParametros = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'perfis_acesso' AND column_name = 'acesso_parametros_sistema'", conn))
                {
                    colunaParametrosExiste = Convert.ToInt32(cmdCheckParametros.ExecuteScalar()) > 0;
                }
                
                string sqlUpdate;
                if (colunaExiste && colunaParametrosExiste)
                {
                    sqlUpdate = @"UPDATE perfis_acesso 
                                 SET nome = @nome, descricao = @descricao, acesso_configuracoes = @acessoConfiguracoes, 
                                     acesso_usuarios = @acessoUsuarios, acesso_projetos = @acessoProjetos, 
                                     acesso_backlog_arquitetura = @acessoBacklogArquitetura, acesso_relatorios = @acessoRelatorios, 
                                     acesso_parametros_sistema = @acessoParametrosSistema, acesso_total = @acessoTotal, ativo = @ativo
                                 WHERE id = @id";
                }
                else if (colunaExiste)
                {
                    sqlUpdate = @"UPDATE perfis_acesso 
                                 SET nome = @nome, descricao = @descricao, acesso_configuracoes = @acessoConfiguracoes, 
                                     acesso_usuarios = @acessoUsuarios, acesso_projetos = @acessoProjetos, 
                                     acesso_backlog_arquitetura = @acessoBacklogArquitetura, acesso_relatorios = @acessoRelatorios, 
                                     acesso_total = @acessoTotal, ativo = @ativo
                                 WHERE id = @id";
                }
                else if (colunaParametrosExiste)
                {
                    sqlUpdate = @"UPDATE perfis_acesso 
                                 SET nome = @nome, descricao = @descricao, acesso_configuracoes = @acessoConfiguracoes, 
                                     acesso_usuarios = @acessoUsuarios, acesso_projetos = @acessoProjetos, 
                                     acesso_relatorios = @acessoRelatorios, acesso_parametros_sistema = @acessoParametrosSistema,
                                     acesso_total = @acessoTotal, ativo = @ativo
                                 WHERE id = @id";
                }
                else
                {
                    sqlUpdate = @"UPDATE perfis_acesso 
                                 SET nome = @nome, descricao = @descricao, acesso_configuracoes = @acessoConfiguracoes, 
                                     acesso_usuarios = @acessoUsuarios, acesso_projetos = @acessoProjetos, 
                                     acesso_relatorios = @acessoRelatorios, acesso_total = @acessoTotal, ativo = @ativo
                                 WHERE id = @id";
                }
                
                using (var cmd = new NpgsqlCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@id", perfilViewModel.Id);
                    cmd.Parameters.AddWithValue("@nome", perfilViewModel.Nome);
                    cmd.Parameters.AddWithValue("@descricao", perfilViewModel.Descricao ?? "");
                    cmd.Parameters.AddWithValue("@acessoConfiguracoes", acessoConfiguracoes);
                    cmd.Parameters.AddWithValue("@acessoUsuarios", acessoUsuarios);
                    cmd.Parameters.AddWithValue("@acessoProjetos", acessoProjetos);
                    if (colunaExiste)
                    {
                        cmd.Parameters.AddWithValue("@acessoBacklogArquitetura", acessoBacklogArquitetura);
                    }
                    cmd.Parameters.AddWithValue("@acessoRelatorios", acessoRelatorios);
                    if (colunaParametrosExiste)
                    {
                        cmd.Parameters.AddWithValue("@acessoParametrosSistema", acessoParametrosSistema);
                    }
                    cmd.Parameters.AddWithValue("@acessoTotal", perfilViewModel.AcessoTotal);
                    cmd.Parameters.AddWithValue("@ativo", perfilViewModel.Ativo);
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ExcluirPerfil(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM perfis_acesso WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Perfil não encontrado ou já foi excluído.");
                    }
                }
            }
        }

        private void VerificarEstruturaTabela()
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                
                // Verificar estrutura da tabela
                System.Diagnostics.Debug.WriteLine("=== ESTRUTURA DA TABELA PERFIS_ACESSO ===");
                
                using (var cmd = new NpgsqlCommand(@"
                    SELECT column_name, data_type, is_nullable, column_default, character_maximum_length
                    FROM information_schema.columns 
                    WHERE table_name = 'perfis_acesso' 
                    ORDER BY ordinal_position", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnName = reader.GetString(0);
                            var dataType = reader.GetString(1);
                            var isNullable = reader.GetString(2);
                            var defaultValue = reader.IsDBNull(3) ? "NULL" : reader.GetString(3);
                            var maxLength = reader.IsDBNull(4) ? "NULL" : reader.GetInt32(4).ToString();
                            
                            System.Diagnostics.Debug.WriteLine($"Coluna: {columnName}, Tipo: {dataType}, Nullable: {isNullable}, Default: {defaultValue}, MaxLength: {maxLength}");
                        }
                    }
                }
                
                // Verificar constraints
                System.Diagnostics.Debug.WriteLine("=== CONSTRAINTS DA TABELA PERFIS_ACESSO ===");
                
                using (var cmd = new NpgsqlCommand(@"
                    SELECT conname, contype, pg_get_constraintdef(oid) as definition
                    FROM pg_constraint 
                    WHERE conrelid = 'perfis_acesso'::regclass", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var constraintName = reader.GetString(0);
                            var constraintType = reader.GetString(1);
                            var definition = reader.GetString(2);
                            
                            System.Diagnostics.Debug.WriteLine($"Constraint: {constraintName}, Tipo: {constraintType}, Definição: {definition}");
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("=============================================");
            }
        }
    }
} 