using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using coc_solucoes_dash.Models;
using Npgsql;
using System;
using System.Collections.Generic;

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