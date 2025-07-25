using System.Collections.Generic;

namespace coc_solucoes_dash.Models
{
    public class Ambiente
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        // Relacionamentos (opcional, se usar EF)
        public ICollection<Segmento> Segmentos { get; set; }
        public ICollection<Meta> Metas { get; set; }
        public ICollection<Incidente> Incidentes { get; set; }
    }
}