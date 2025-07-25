using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using coc_solucoes_dash.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Dashboard.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(IConfiguration configuration) : base(configuration) { }

        public IActionResult Index(int? ambienteId = null, int? segmentoId = null, int? ano = null, int? mes = null)
        {
            // Exemplo: buscar métricas e dados do dashboard
            var dashboardService = new Services.DashboardService(_configuration);
            var viewModel = dashboardService.ObterDashboardViewModel(ambienteId, segmentoId, ano, mes);

            // Buscar dias com incidentes para o calendário
            var incidenteService = new Services.IncidenteService(_configuration);
            List<(DateTime Dia, int CriticidadeId, int CriticidadePeso, string CriticidadeCor)> diasComIncidentes;
            if (ambienteId.HasValue)
            {
                // Se ambiente selecionado, mostrar todos os meses do ano
                diasComIncidentes = incidenteService.ObterDiasComIncidentes(ano, null, ambienteId, segmentoId);
            }
            else
            {
                // Sem ambiente, mostrar só o mês atual de todos os ambientes
                int anoAtual = DateTime.Now.Year;
                int mesAtual = DateTime.Now.Month;
                diasComIncidentes = incidenteService.ObterDiasComIncidentes(anoAtual, mesAtual, null, null);
            }
            ViewBag.DiasComIncidentes = diasComIncidentes.Select(x => new {
                Dia = x.Dia,
                CriticidadeId = x.CriticidadeId,
                CriticidadePeso = x.CriticidadePeso,
                CriticidadeCor = x.CriticidadeCor
            }).ToList();
            
            ViewBag.AnoSelecionado = ambienteId.HasValue ? (ano ?? DateTime.Now.Year) : DateTime.Now.Year;
            ViewBag.MesSelecionado = ambienteId.HasValue ? (mes ?? DateTime.Now.Month) : DateTime.Now.Month;
            ViewBag.AmbienteSelecionado = ambienteId;
            // Buscar ambientes e segmentos para os filtros
            ViewBag.Ambientes = incidenteService.ObterAmbientes();
            ViewBag.Segmentos = incidenteService.ObterTodosSegmentos();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult NovoIncidente()
        {
            // Redireciona para o formulário de novo incidente
            return RedirectToAction("Create", "Incidente");
        }
    }
} 