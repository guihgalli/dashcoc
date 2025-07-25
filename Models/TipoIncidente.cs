using System.Collections.Generic;

namespace coc_solucoes_dash.Models
{
    public class TipoIncidente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public ICollection<Incidente> Incidentes { get; set; }
    }
} 