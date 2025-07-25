using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using coc_solucoes_dash.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Dashboard.Controllers
{
    public class CadastroController : BaseController
    {
        public CadastroController(IConfiguration configuration) : base(configuration) { }

        public IActionResult Index(string aba = "Ambientes")
        {
            var cadastroService = new Services.CadastroService(_configuration);
            ViewBag.AbaAtiva = aba;
            ViewBag.Ambientes = cadastroService.ObterAmbientes();
            ViewBag.Segmentos = cadastroService.ObterSegmentos();
            ViewBag.TiposIncidente = cadastroService.ObterTiposIncidente();
            ViewBag.Criticidades = cadastroService.ObterCriticidades();
            ViewBag.Metas = cadastroService.ObterMetas();
            return View();
        }

        // Métodos para CRUD de cada entidade (Ambiente, Segmento, TipoIncidente, Criticidade, Meta)
        // Exemplo para Ambiente:
        [HttpPost]
        public IActionResult CriarAmbiente(Ambiente ambiente)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.InserirAmbiente(ambiente);
            // Regra: criar segmento SEG01 automaticamente
            cadastroService.CriarSegmentoPadraoParaAmbiente(ambiente);
            TempData["Sucesso"] = "Ambiente criado com sucesso!";
            return RedirectToAction("Index", new { aba = "Ambientes" });
        }

        [HttpPost]
        public IActionResult EditarAmbiente(Ambiente ambiente)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.AtualizarAmbiente(ambiente);
            TempData["Sucesso"] = "Ambiente atualizado com sucesso!";
            return RedirectToAction("Index", new { aba = "Ambientes" });
        }

        [HttpPost]
        public IActionResult ExcluirAmbiente(int id)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.ExcluirAmbiente(id);
            TempData["Sucesso"] = "Ambiente excluído com sucesso!";
            return RedirectToAction("Index", new { aba = "Ambientes" });
        }

        [HttpPost]
        public IActionResult EditarSegmento(Segmento segmento)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.AtualizarSegmento(segmento);
            TempData["Sucesso"] = "Segmento atualizado com sucesso!";
            return RedirectToAction("Index", new { aba = "Segmentos" });
        }

        [HttpPost]
        public IActionResult ExcluirSegmento(int id)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.ExcluirSegmento(id);
            TempData["Sucesso"] = "Segmento excluído com sucesso!";
            return RedirectToAction("Index", new { aba = "Segmentos" });
        }

        [HttpPost]
        public IActionResult EditarTipoIncidente(TipoIncidente tipo)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.AtualizarTipoIncidente(tipo);
            TempData["Sucesso"] = "Tipo de Incidente atualizado com sucesso!";
            return RedirectToAction("Index", new { aba = "TiposIncidente" });
        }

        [HttpPost]
        public IActionResult ExcluirTipoIncidente(int id)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.ExcluirTipoIncidente(id);
            TempData["Sucesso"] = "Tipo de Incidente excluído com sucesso!";
            return RedirectToAction("Index", new { aba = "TiposIncidente" });
        }

        [HttpPost]
        public IActionResult EditarCriticidade(Criticidade crit)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.AtualizarCriticidade(crit);
            TempData["Sucesso"] = "Criticidade atualizada com sucesso!";
            return RedirectToAction("Index", new { aba = "Criticidades" });
        }

        [HttpPost]
        public IActionResult ExcluirCriticidade(int id)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.ExcluirCriticidade(id);
            TempData["Sucesso"] = "Criticidade excluída com sucesso!";
            return RedirectToAction("Index", new { aba = "Criticidades" });
        }

        [HttpPost]
        public IActionResult EditarMeta(Meta meta)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.AtualizarMeta(meta);
            TempData["Sucesso"] = "Meta atualizada com sucesso!";
            return RedirectToAction("Index", new { aba = "Metas" });
        }

        [HttpPost]
        public IActionResult ExcluirMeta(int id)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.ExcluirMeta(id);
            TempData["Sucesso"] = "Meta excluída com sucesso!";
            return RedirectToAction("Index", new { aba = "Metas" });
        }

        [HttpPost]
        public IActionResult CriarTipoIncidente(TipoIncidente tipo)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.InserirTipoIncidente(tipo);
            TempData["Sucesso"] = "Tipo de Incidente criado com sucesso!";
            return RedirectToAction("Index", new { aba = "TiposIncidente" });
        }

        [HttpPost]
        public IActionResult CriarMeta(Meta meta)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.InserirMeta(meta);
            TempData["Sucesso"] = "Meta criada com sucesso!";
            return RedirectToAction("Index", new { aba = "Metas" });
        }

        [HttpPost]
        public IActionResult CriarSegmento(Segmento segmento)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.InserirSegmento(segmento);
            TempData["Sucesso"] = "Segmento criado com sucesso!";
            return RedirectToAction("Index", new { aba = "Segmentos" });
        }

        [HttpPost]
        public IActionResult CriarCriticidade(Criticidade crit)
        {
            var cadastroService = new Services.CadastroService(_configuration);
            cadastroService.InserirCriticidade(crit);
            TempData["Sucesso"] = "Criticidade criada com sucesso!";
            return RedirectToAction("Index", new { aba = "Criticidades" });
        }
    }
} 