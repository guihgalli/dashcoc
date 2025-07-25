using coc_solucoes_dash.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dashboard.Services
{
    public class DashboardService
    {
        private readonly IConfiguration _configuration;
        public DashboardService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DashboardViewModel ObterDashboardViewModel(int? ambienteId, int? segmentoId, int? ano, int? mes)
        {
            string? connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connString)) return new DashboardViewModel();

            double mttr = 0, mtbf = 0, disponibilidade = 100;
            int total = 0, abertos = 0, criticos = 0;
            var incidentes = new List<(DateTime inicio, DateTime? fim, int criticidadeId, int? duracao)>();

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                var sql = @"SELECT datahorainicio, datahorafim, criticidadeid, duracaominutos FROM incidentes WHERE 1=1";
                if (ambienteId.HasValue) sql += " AND ambienteid = @ambienteId";
                if (segmentoId.HasValue) sql += " AND segmentoid = @segmentoId";
                if (ano.HasValue) sql += " AND EXTRACT(YEAR FROM datahorainicio) = @ano";
                if (mes.HasValue) sql += " AND EXTRACT(MONTH FROM datahorainicio) = @mes";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (ambienteId.HasValue) cmd.Parameters.AddWithValue("@ambienteId", ambienteId.Value);
                    if (segmentoId.HasValue) cmd.Parameters.AddWithValue("@segmentoId", segmentoId.Value);
                    if (ano.HasValue) cmd.Parameters.AddWithValue("@ano", ano.Value);
                    if (mes.HasValue) cmd.Parameters.AddWithValue("@mes", mes.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            incidentes.Add((
                                reader.GetDateTime(0),
                                reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                                reader.GetInt32(2),
                                reader.IsDBNull(3) ? null : reader.GetInt32(3)
                            ));
                        }
                    }
                }
            }

            total = incidentes.Count;
            abertos = incidentes.FindAll(i => !i.fim.HasValue).Count;
            criticos = incidentes.FindAll(i => i.criticidadeId == 1).Count; // Supondo 1 = Crítico

            var resolvidos = incidentes.FindAll(i => i.fim.HasValue && i.duracao.HasValue);
            if (resolvidos.Count > 0)
                mttr = Math.Round(resolvidos.Average(i => i.duracao.Value), 2);

            if (resolvidos.Count > 1)
            {
                var ordered = resolvidos.OrderBy(i => i.inicio).ToList();
                var temposEntreFalhas = new List<double>();
                for (int i = 1; i < ordered.Count; i++)
                {
                    var diff = (ordered[i].inicio - ordered[i - 1].fim.Value).TotalMinutes;
                    if (diff > 0) temposEntreFalhas.Add(diff);
                }
                if (temposEntreFalhas.Count > 0)
                    mtbf = Math.Round(temposEntreFalhas.Average(), 2);
            }

            // Disponibilidade simplificada: 100 - (soma dos downtimes / tempo total do período)
            if (resolvidos.Count > 0)
            {
                double downtime = resolvidos.Sum(i => i.duracao.Value);
                double periodo = 30 * 24 * 60; // 30 dias em minutos (ajustar para o período real)
                disponibilidade = Math.Round(100 - (downtime / periodo * 100), 2);
            }

            return new DashboardViewModel
            {
                MTTR = mttr,
                MTBF = mtbf,
                DisponibilidadeMedia = disponibilidade,
                TotalIncidentes = total,
                IncidentesAbertos = abertos,
                IncidentesCriticos = criticos
            };
        }
    }

    public class DashboardViewModel
    {
        public double MTTR { get; set; }
        public double MTBF { get; set; }
        public double DisponibilidadeMedia { get; set; }
        public int TotalIncidentes { get; set; }
        public int IncidentesAbertos { get; set; }
        public int IncidentesCriticos { get; set; }
    }
} 