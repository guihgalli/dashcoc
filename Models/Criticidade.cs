using System.Collections.Generic;

namespace coc_solucoes_dash.Models
{
    public class Criticidade
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; } // RGB
        public int Peso { get; set; }
        public bool Downtime { get; set; }
        public string Descricao { get; set; }
        public ICollection<Incidente> Incidentes { get; set; }
    }
} 