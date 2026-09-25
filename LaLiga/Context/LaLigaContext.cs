using LaLiga.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Context
{
    public class LaLigaContext : DbContext
    {
        public LaLigaContext(DbContextOptions<LaLigaContext> options) : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchGoal> MatchGoals { get; set; }
        public DbSet<MatchCard> MatchCards { get; set; }
        public DbSet<Substitution> Substitutions { get; set; }
        public DbSet<MatchStatistic> MatchStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Takım adı ve kısa adı tekil olmalı
            modelBuilder.Entity<Team>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<Team>().HasIndex(t => t.Code).IsUnique();

            
            modelBuilder.Entity<Match>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchStatistic>()
                .HasOne(s => s.Match)
                .WithOne(m => m.Statistic)
                .HasForeignKey<MatchStatistic>(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Gol / Kart / Değişiklik: maç silinince olayları da silinir, takım silinmesi engellenir
            modelBuilder.Entity<MatchGoal>()
                .HasOne(g => g.Match)
                .WithMany(m => m.Goals)
                .HasForeignKey(g => g.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchGoal>()
                .HasOne(g => g.Team)
                .WithMany()
                .HasForeignKey(g => g.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchCard>()
                .HasOne(c => c.Match)
                .WithMany(m => m.Cards)
                .HasForeignKey(c => c.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchCard>()
                .HasOne(c => c.Team)
                .WithMany()
                .HasForeignKey(c => c.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Substitution>()
                .HasOne(s => s.Match)
                .WithMany(m => m.Substitutions)
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Substitution>()
                .HasOne(s => s.Team)
                .WithMany()
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
