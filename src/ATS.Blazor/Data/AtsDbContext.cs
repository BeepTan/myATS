using Microsoft.EntityFrameworkCore;
using ATS.Blazor.Data.Entities;

namespace ATS.Blazor.Data;

public class AtsDbContext : DbContext
{
    public AtsDbContext(DbContextOptions<AtsDbContext> options) : base(options)
    {
    }

    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<EvaluationResult> EvaluationResults => Set<EvaluationResult>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One-to-One between Application and EvaluationResult
        modelBuilder.Entity<Application>()
            .HasOne(a => a.EvaluationResult)
            .WithOne(e => e.Application)
            .HasForeignKey<EvaluationResult>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade delete applications when job is removed
        modelBuilder.Entity<JobPosting>()
            .HasMany(j => j.Applications)
            .WithOne(a => a.JobPosting)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed initial default job posting
        var sampleJobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<JobPosting>().HasData(new JobPosting
        {
            Id = sampleJobId,
            Title = "Senior Full-Stack Engineer (.NET & Python)",
            Department = "Product Engineering",
            Description = "We are seeking an experienced Full-Stack Engineer proficient in C#, .NET 9.0, and Python microservices to design, scale, and evaluate distributed cloud applications.",
            RequiredSkills = "C#, .NET, Python, FastAPI, SQLite, Docker, Cloud",
            MinPassingScore = 75.0,
            CriteriaJson = "[{\"category\":\"Technical Skills\",\"weight_percentage\":40.0,\"instructions\":\"Assess hands-on proficiency in .NET Core and Python\"},{\"category\":\"Relevant Experience\",\"weight_percentage\":35.0,\"instructions\":\"Evaluate track record in scalable web architectures\"},{\"category\":\"Education & Certs\",\"weight_percentage\":25.0,\"instructions\":\"Review computer science background and relevant cloud certifications\"}]",
            IsActive = true,
            CreatedAt = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
