using System.Collections.Generic;

namespace coc_solucoes_dash.Models
{
    public class Segmento
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int AmbienteId { get; set; }
        public Ambiente Ambiente { get; set; }
        public ICollection<Meta> Metas { get; set; }
        public ICollection<Incidente> Incidentes { get; set; }
    }
} 