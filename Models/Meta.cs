namespace coc_solucoes_dash.Models
{
    public class Meta
    {
        public int Id { get; set; }
        public int AmbienteId { get; set; }
        public Ambiente Ambiente { get; set; }
        public int SegmentoId { get; set; }
        public Segmento Segmento { get; set; }
        public int Peso { get; set; } // %
        public double MTTRMetaHoras { get; set; }
        public bool SuperacaoMTTR { get; set; }
        public double MTBFMetaHoras { get; set; }
        public bool SuperacaoMTBF { get; set; }
        public double MTBFMetaDias { get; set; }
        public double DisponibilidadeMeta { get; set; } // %
    }
} 