using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace coc_solucoes_dash.Models
{
    public class Incidente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Data/Hora de início é obrigatória")]
        public DateTime DataHoraInicio { get; set; }

        public DateTime? DataHoraFim { get; set; }

        [Required(ErrorMessage = "Tipo de incidente é obrigatório")]
        public int TipoIncidenteId { get; set; }
        [ValidateNever]
        public TipoIncidente TipoIncidente { get; set; }

        [Required(ErrorMessage = "Ambiente é obrigatório")]
        public int AmbienteId { get; set; }
        [ValidateNever]
        public Ambiente Ambiente { get; set; }

        [Required(ErrorMessage = "Segmento é obrigatório")]
        public int SegmentoId { get; set; }
        [ValidateNever]
        public Segmento Segmento { get; set; }

        [Required(ErrorMessage = "Criticidade é obrigatória")]
        public int CriticidadeId { get; set; }
        [ValidateNever]
        public Criticidade Criticidade { get; set; }

        [Required(ErrorMessage = "Descrição é obrigatória")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Ações tomadas são obrigatórias")]
        public string AcoesTomadas { get; set; }

        public int? DuracaoMinutos { get; set; }
    }
}
