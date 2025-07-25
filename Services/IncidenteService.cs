using coc_solucoes_dash.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Dashboard.Services
{
    public class IncidenteService
    {
        private readonly IConfiguration _configuration;
        public IncidenteService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Incidente> ObterTodos()
        {
            var incidentes = new List<Incidente>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return incidentes;

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT i.id, i.datahorainicio, i.datahorafim, i.tipoincidenteid, t.nome as tiponome, i.ambienteid, a.nome as ambientenome, i.segmentoid, s.nome as segmentonome, i.criticidadeid, c.nome as criticidadenome, c.cor, i.descricao, i.acoestomadas, i.duracaominutos
                    FROM incidentes i
                    LEFT JOIN ambientes a ON i.ambienteid = a.id
                    LEFT JOIN segmentos s ON i.segmentoid = s.id
                    LEFT JOIN tiposincidente t ON i.tipoincidenteid = t.id
                    LEFT JOIN criticidades c ON i.criticidadeid = c.id
                    ORDER BY i.datahorainicio DESC
                ", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            incidentes.Add(new Incidente
                            {
                                Id = reader.GetInt32(0),
                                DataHoraInicio = reader.GetDateTime(1),
                                DataHoraFim = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                TipoIncidenteId = reader.GetInt32(3),
                                TipoIncidente = new TipoIncidente { Id = reader.GetInt32(3), Nome = reader.IsDBNull(4) ? null : reader.GetString(4) },
                                AmbienteId = reader.GetInt32(5),
                                Ambiente = new Ambiente { Id = reader.GetInt32(5), Nome = reader.IsDBNull(6) ? null : reader.GetString(6) },
                                SegmentoId = reader.GetInt32(7),
                                Segmento = new Segmento { Id = reader.GetInt32(7), Nome = reader.IsDBNull(8) ? null : reader.GetString(8) },
                                CriticidadeId = reader.GetInt32(9),
                                Criticidade = new Criticidade {
                                    Id = reader.GetInt32(9),
                                    Nome = reader.IsDBNull(10) ? null : reader.GetString(10),
                                    Cor = reader.IsDBNull(11) ? null : reader.GetString(11)
                                },
                                Descricao = reader.IsDBNull(12) ? null : reader.GetString(12),
                                AcoesTomadas = reader.IsDBNull(13) ? null : reader.GetString(13),
                                DuracaoMinutos = reader.IsDBNull(14) ? null : reader.GetInt32(14)
                            });
                        }
                    }
                }
            }
            return incidentes;
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

        public Incidente? ObterPorId(int id)
        {
            var incidentes = ObterTodos();
            return incidentes.Find(i => i.Id == id);
        }

        public void Inserir(Incidente incidente)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO incidentes (datahorainicio, datahorafim, tipoincidenteid, ambienteid, segmentoid, criticidadeid, descricao, acoestomadas, duracaominutos)
                    VALUES (@inicio, @fim, @tipo, @ambiente, @segmento, @criticidade, @descricao, @acoes, @duracao)
                ", conn))
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                    var inicioBrasilia = TimeZoneInfo.ConvertTimeFromUtc(incidente.DataHoraInicio.ToUniversalTime(), tz);
                    var fimBrasilia = incidente.DataHoraFim.HasValue ? (object)TimeZoneInfo.ConvertTimeFromUtc(incidente.DataHoraFim.Value.ToUniversalTime(), tz) : DBNull.Value;
                    cmd.Parameters.AddWithValue("@inicio", inicioBrasilia);
                    cmd.Parameters.AddWithValue("@fim", fimBrasilia);
                    cmd.Parameters.AddWithValue("@tipo", incidente.TipoIncidenteId);
                    cmd.Parameters.AddWithValue("@ambiente", incidente.AmbienteId);
                    cmd.Parameters.AddWithValue("@segmento", incidente.SegmentoId);
                    cmd.Parameters.AddWithValue("@criticidade", incidente.CriticidadeId);
                    cmd.Parameters.AddWithValue("@descricao", incidente.Descricao ?? "");
                    cmd.Parameters.AddWithValue("@acoes", incidente.AcoesTomadas ?? "");
                    cmd.Parameters.AddWithValue("@duracao", (object?)incidente.DuracaoMinutos ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Atualizar(Incidente incidente)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE incidentes SET datahorainicio=@inicio, datahorafim=@fim, tipoincidenteid=@tipo, ambienteid=@ambiente, segmentoid=@segmento, criticidadeid=@criticidade, descricao=@descricao, acoestomadas=@acoes, duracaominutos=@duracao
                    WHERE id=@id
                ", conn))
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                    var inicioBrasilia = TimeZoneInfo.ConvertTimeFromUtc(incidente.DataHoraInicio.ToUniversalTime(), tz);
                    var fimBrasilia = incidente.DataHoraFim.HasValue ? (object)TimeZoneInfo.ConvertTimeFromUtc(incidente.DataHoraFim.Value.ToUniversalTime(), tz) : DBNull.Value;
                    cmd.Parameters.AddWithValue("@id", incidente.Id);
                    cmd.Parameters.AddWithValue("@inicio", inicioBrasilia);
                    cmd.Parameters.AddWithValue("@fim", fimBrasilia);
                    cmd.Parameters.AddWithValue("@tipo", incidente.TipoIncidenteId);
                    cmd.Parameters.AddWithValue("@ambiente", incidente.AmbienteId);
                    cmd.Parameters.AddWithValue("@segmento", incidente.SegmentoId);
                    cmd.Parameters.AddWithValue("@criticidade", incidente.CriticidadeId);
                    cmd.Parameters.AddWithValue("@descricao", incidente.Descricao ?? "");
                    cmd.Parameters.AddWithValue("@acoes", incidente.AcoesTomadas ?? "");
                    cmd.Parameters.AddWithValue("@duracao", (object?)incidente.DuracaoMinutos ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Excluir(int id)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM incidentes WHERE id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Segmento> ObterSegmentosPorAmbiente(int ambienteId)
        {
            var segmentos = new List<Segmento>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return segmentos;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, nome FROM segmentos WHERE ambienteid = @ambienteId ORDER BY nome", conn))
                {
                    cmd.Parameters.AddWithValue("@ambienteId", ambienteId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            segmentos.Add(new Segmento { Id = reader.GetInt32(0), Nome = reader.GetString(1), AmbienteId = ambienteId });
                        }
                    }
                }
            }
            return segmentos;
        }

        public List<Segmento> ObterTodosSegmentos()
        {
            var segmentos = new List<Segmento>();
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return segmentos;
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT s.id, s.nome, s.ambienteid, a.nome as ambientenome 
                    FROM segmentos s 
                    LEFT JOIN ambientes a ON s.ambienteid = a.id 
                    ORDER BY a.nome, s.nome", conn))
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
    }
} 