using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using coc_solucoes_dash.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Dashboard.Controllers
{
    public class IncidenteController : BaseController
    {
        public IncidenteController(IConfiguration configuration) : base(configuration) { }

        public IActionResult Index()
        {
            var incidenteService = new Services.IncidenteService(_configuration);
            var incidentes = incidenteService.ObterTodos();
            ViewBag.Criticidades = incidenteService.ObterCriticidades();
            ViewBag.TiposIncidente = incidenteService.ObterTiposIncidente();
            return View(incidentes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var incidenteService = new Services.IncidenteService(_configuration);
            ViewBag.Ambientes = incidenteService.ObterAmbientes();
            ViewBag.TiposIncidente = incidenteService.ObterTiposIncidente();
            ViewBag.Criticidades = incidenteService.ObterCriticidades();
            ViewBag.Segmentos = incidenteService.ObterTodosSegmentos();
            ViewBag.SegmentosJson = JsonConvert.SerializeObject(ViewBag.Segmentos);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Incidente incidente)
        {
            // LOG TEMPORÁRIO PARA DEPURAÇÃO
            Console.WriteLine($"DataHoraInicio: {incidente.DataHoraInicio}");
            Console.WriteLine($"DataHoraFim: {incidente.DataHoraFim}");
            Console.WriteLine($"TipoIncidenteId: {incidente.TipoIncidenteId}");
            Console.WriteLine($"AmbienteId: {incidente.AmbienteId}");
            Console.WriteLine($"SegmentoId: {incidente.SegmentoId}");
            Console.WriteLine($"CriticidadeId: {incidente.CriticidadeId}");
            Console.WriteLine($"Descricao: {incidente.Descricao}");
            Console.WriteLine($"AcoesTomadas: {incidente.AcoesTomadas}");
            Console.WriteLine($"DuracaoMinutos: {incidente.DuracaoMinutos}");
            var service = new Services.IncidenteService(_configuration);
            if (ModelState.IsValid)
            {
                try {
                    service.Inserir(incidente);
                    TempData["Sucesso"] = "Incidente registrado com sucesso!";
                    return RedirectToAction("Index");
                } catch (Exception ex) {
                    TempData["Erro"] = "Erro ao registrar incidente: " + ex.Message;
                }
            }
            else
            {
                // LOG DETALHADO: Mostrar todas as chaves do ModelState, valores e erros
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    var attemptedValue = entry?.AttemptedValue;
                    Console.WriteLine($"ModelState Key: {key} | AttemptedValue: {attemptedValue}");
                    foreach (var error in entry.Errors)
                    {
                        Console.WriteLine($"  Erro: {error.ErrorMessage}");
                    }
                }
                // LOG TEMPORÁRIO: Exibir todos os erros do ModelState
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Erro em {key}: {error.ErrorMessage}");
                    }
                }
                TempData["Erro"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            // Preencher ViewBags corretamente após erro de validação
            ViewBag.Ambientes = service.ObterAmbientes();
            ViewBag.TiposIncidente = service.ObterTiposIncidente();
            ViewBag.Criticidades = service.ObterCriticidades();
            // Preencher segmentos conforme ambiente selecionado, se houver
            if (incidente.AmbienteId > 0)
            {
                ViewBag.Segmentos = service.ObterSegmentosPorAmbiente(incidente.AmbienteId);
            }
            else
            {
                ViewBag.Segmentos = service.ObterTodosSegmentos();
            }
            ViewBag.SegmentosJson = JsonConvert.SerializeObject(ViewBag.Segmentos);
            return View(incidente);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var incidenteService = new Services.IncidenteService(_configuration);
            var incidente = incidenteService.ObterPorId(id);
            if (incidente == null)
            {
                TempData["Erro"] = "Incidente não encontrado.";
                return RedirectToAction("Index");
            }
            ViewBag.Ambientes = incidenteService.ObterAmbientes();
            ViewBag.TiposIncidente = incidenteService.ObterTiposIncidente();
            ViewBag.Criticidades = incidenteService.ObterCriticidades();
            return View(incidente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Incidente incidente)
        {
            if (ModelState.IsValid)
            {
                var incidenteService = new Services.IncidenteService(_configuration);
                incidenteService.Atualizar(incidente);
                TempData["Sucesso"] = "Incidente atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            var service = new Services.IncidenteService(_configuration);
            ViewBag.Ambientes = service.ObterAmbientes();
            ViewBag.TiposIncidente = service.ObterTiposIncidente();
            ViewBag.Criticidades = service.ObterCriticidades();
            return View(incidente);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var incidenteService = new Services.IncidenteService(_configuration);
            incidenteService.Excluir(id);
            return Json(new { success = true, message = "Incidente excluído com sucesso!" });
        }

        [HttpGet]
        public JsonResult SegmentosPorAmbiente(int ambienteId)
        {
            var incidenteService = new Services.IncidenteService(_configuration);
            var segmentos = incidenteService.ObterSegmentosPorAmbiente(ambienteId);
            return Json(segmentos);
        }
    }
} 