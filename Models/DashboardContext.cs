using Microsoft.EntityFrameworkCore;

namespace coc_solucoes_dash.Models
{
    public class DashboardContext : DbContext
    {
        public DashboardContext(DbContextOptions<DashboardContext> options) : base(options) { }

        public DbSet<Ambiente> Ambientes { get; set; }
        public DbSet<Segmento> Segmentos { get; set; }
        public DbSet<TipoIncidente> TiposIncidente { get; set; }
        public DbSet<Criticidade> Criticidades { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<Incidente> Incidentes { get; set; }
    }
} 