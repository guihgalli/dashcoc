using coc_solucoes_dash.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Dashboard.Services
{
    public class CadastroService
    {
        private readonly IConfiguration _configuration;
        public CadastroService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Ambiente> ObterAmbientes()
        {
            var ambientes = new List<Ambiente>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return ambientes;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, nome FROM ambientes ORDER BY nome", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ambientes.Add(new Ambiente { Id = reader.GetInt32(0), Nome = reader.GetString(1) });
                    }
                }
            }
            return ambientes;
        }

        public List<Segmento> ObterSegmentos()
        {
            var segmentos = new List<Segmento>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return segmentos;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT s.id, s.nome, s.ambienteid, a.nome as ambientenome FROM segmentos s LEFT JOIN ambientes a ON s.ambienteid = a.id ORDER BY s.nome", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        segmentos.Add(new Segmento
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            AmbienteId = reader.GetInt32(2),
                            Ambiente = new Ambiente { Id = reader.GetInt32(2), Nome = reader.IsDBNull(3) ? null : reader.GetString(3) }
                        });
                    }
                }
            }
            return segmentos;
        }

        public void InserirAmbiente(Ambiente ambiente)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO ambientes (nome) VALUES (@nome) RETURNING id", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", ambiente.Nome);
                    var id = cmd.ExecuteScalar();
                    if (id != null)
                        ambiente.Id = Convert.ToInt32(id);
                }
            }
        }

        public void CriarSegmentoPadraoParaAmbiente(Ambiente ambiente)
        {
            var segmento = new Segmento { Nome = "SEG01", AmbienteId = ambiente.Id };
            InserirSegmento(segmento);
        }

        public void InserirSegmento(Segmento segmento)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO segmentos (nome, ambienteid) VALUES (@nome, @ambienteid)", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", segmento.Nome);
                    cmd.Parameters.AddWithValue("@ambienteid", segmento.AmbienteId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<TipoIncidente> ObterTiposIncidente()
        {
            var tipos = new List<TipoIncidente>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return tipos;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, nome, descricao FROM tiposincidente ORDER BY nome", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tipos.Add(new TipoIncidente { Id = reader.GetInt32(0), Nome = reader.GetString(1), Descricao = reader.GetString(2) });
                    }
                }
            }
            return tipos;
        }

        public void InserirTipoIncidente(TipoIncidente tipo)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO tiposincidente (nome, descricao) VALUES (@nome, @descricao)", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", tipo.Nome);
                    cmd.Parameters.AddWithValue("@descricao", tipo.Descricao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Criticidade> ObterCriticidades()
        {
            var crits = new List<Criticidade>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return crits;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, nome, cor, peso, downtime, descricao FROM criticidades ORDER BY nome", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        crits.Add(new Criticidade
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Cor = reader.GetString(2),
                            Peso = reader.GetInt32(3),
                            Downtime = reader.GetBoolean(4),
                            Descricao = reader.GetString(5)
                        });
                    }
                }
            }
            return crits;
        }

        public void InserirCriticidade(Criticidade crit)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO criticidades (nome, cor, peso, downtime, descricao) VALUES (@nome, @cor, @peso, @downtime, @descricao)", conn))
                {
                    cmd.Parameters.AddWithValue("@nome", crit.Nome);
                    cmd.Parameters.AddWithValue("@cor", crit.Cor);
                    cmd.Parameters.AddWithValue("@peso", crit.Peso);
                    cmd.Parameters.AddWithValue("@downtime", crit.Downtime);
                    cmd.Parameters.AddWithValue("@descricao", crit.Descricao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Meta> ObterMetas()
        {
            var metas = new List<Meta>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return metas;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"SELECT m.id, m.ambienteid, a.nome, m.segmentoid, s.nome, m.peso, m.mttrmetahoras, m.superacaomttr, m.mtbfmetahoras, m.superacaomtbf, m.mtbfmetadias, m.disponibilidademeta FROM metas m LEFT JOIN ambientes a ON m.ambienteid = a.id LEFT JOIN segmentos s ON m.segmentoid = s.id ORDER BY m.id", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        metas.Add(new Meta
                        {
                            Id = reader.GetInt32(0),
                            AmbienteId = reader.GetInt32(1),
                            Ambiente = new Ambiente { Id = reader.GetInt32(1), Nome = reader.IsDBNull(2) ? null : reader.GetString(2) },
                            SegmentoId = reader.GetInt32(3),
                            Segmento = new Segmento { Id = reader.GetInt32(3), Nome = reader.IsDBNull(4) ? null : reader.GetString(4) },
                            Peso = reader.GetInt32(5),
                            MTTRMetaHoras = reader.GetDouble(6),
                            SuperacaoMTTR = reader.GetBoolean(7),
                            MTBFMetaHoras = reader.GetDouble(8),
                            SuperacaoMTBF = reader.GetBoolean(9),
                            MTBFMetaDias = reader.GetDouble(10),
                            DisponibilidadeMeta = reader.GetDouble(11)
                        });
                    }
                }
            }
            return metas;
        }

        public void InserirMeta(Meta meta)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO metas (ambienteid, segmentoId, peso, mttrmetahoras, superacaomttr, mtbfmetahoras, superacaomtbf, mtbfmetadias, disponibilidademeta) VALUES (@ambienteid, @segmentoid, @peso, @mttr, @supmttr, @mtbf, @supmtbf, @mtbfdias, @disp)", conn))
                {
                    cmd.Parameters.AddWithValue("@ambienteid", meta.AmbienteId);
                    cmd.Parameters.AddWithValue("@segmentoid", meta.SegmentoId);
                    cmd.Parameters.AddWithValue("@peso", meta.Peso);
                    cmd.Parameters.AddWithValue("@mttr", meta.MTTRMetaHoras);
                    cmd.Parameters.AddWithValue("@supmttr", meta.SuperacaoMTTR);
                    cmd.Parameters.AddWithValue("@mtbf", meta.MTBFMetaHoras);
                    cmd.Parameters.AddWithValue("@supmtbf", meta.SuperacaoMTBF);
                    cmd.Parameters.AddWithValue("@mtbfdias", meta.MTBFMetaDias);
                    cmd.Parameters.AddWithValue("@disp", meta.DisponibilidadeMeta);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AtualizarAmbiente(Ambiente ambiente)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE ambientes SET nome=@nome WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", ambiente.Id);
                    cmd.Parameters.AddWithValue("@nome", ambiente.Nome);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExcluirAmbiente(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM ambientes WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AtualizarSegmento(Segmento segmento)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE segmentos SET nome=@nome, ambienteid=@ambienteid WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", segmento.Id);
                    cmd.Parameters.AddWithValue("@nome", segmento.Nome);
                    cmd.Parameters.AddWithValue("@ambienteid", segmento.AmbienteId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExcluirSegmento(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM segmentos WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AtualizarTipoIncidente(TipoIncidente tipo)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE tiposincidente SET nome=@nome, descricao=@descricao WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tipo.Id);
                    cmd.Parameters.AddWithValue("@nome", tipo.Nome);
                    cmd.Parameters.AddWithValue("@descricao", tipo.Descricao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExcluirTipoIncidente(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM tiposincidente WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AtualizarCriticidade(Criticidade crit)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE criticidades SET nome=@nome, cor=@cor, peso=@peso, downtime=@downtime, descricao=@descricao WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", crit.Id);
                    cmd.Parameters.AddWithValue("@nome", crit.Nome);
                    cmd.Parameters.AddWithValue("@cor", crit.Cor);
                    cmd.Parameters.AddWithValue("@peso", crit.Peso);
                    cmd.Parameters.AddWithValue("@downtime", crit.Downtime);
                    cmd.Parameters.AddWithValue("@descricao", crit.Descricao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExcluirCriticidade(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM criticidades WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AtualizarMeta(Meta meta)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE metas SET ambienteid=@ambienteid, segmentoId=@segmentoid, peso=@peso, mttrmetahoras=@mttr, superacaomttr=@supmttr, mtbfmetahoras=@mtbf, superacaomtbf=@supmtbf, mtbfmetadias=@mtbfdias, disponibilidademeta=@disp WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", meta.Id);
                    cmd.Parameters.AddWithValue("@ambienteid", meta.AmbienteId);
                    cmd.Parameters.AddWithValue("@segmentoid", meta.SegmentoId);
                    cmd.Parameters.AddWithValue("@peso", meta.Peso);
                    cmd.Parameters.AddWithValue("@mttr", meta.MTTRMetaHoras);
                    cmd.Parameters.AddWithValue("@supmttr", meta.SuperacaoMTTR);
                    cmd.Parameters.AddWithValue("@mtbf", meta.MTBFMetaHoras);
                    cmd.Parameters.AddWithValue("@supmtbf", meta.SuperacaoMTBF);
                    cmd.Parameters.AddWithValue("@mtbfdias", meta.MTBFMetaDias);
                    cmd.Parameters.AddWithValue("@disp", meta.DisponibilidadeMeta);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExcluirMeta(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM metas WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
} 