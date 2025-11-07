using COMP2139_ICE1.Areas.ProjectManagement.Models;
using COMP2139_ICE1.Models;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    
    public DbSet<ProjectComment> ProjectComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Define One_to_Many Relationship: One Project has Many ProjectTasks
        modelBuilder.Entity<Project>()
            
            // One Project has many ProjectTasks
            .HasMany(p => p.Tasks)

            // Each ProjectTask belongs to one Project
            .WithOne(t => t.Project)

            // Foreign Key in ProjectTask table
            .HasForeignKey(t => t.ProjectId)

            .OnDelete(DeleteBehavior.Cascade);
        
        // Cascade delete ProjectTasks when a Project is deleted
        // Seeding Projects
        modelBuilder.Entity<Project>().HasData(
            new Project { ProjectId = 1, Name = "Assignment 1", Description = "COMP2139 Assignment 1" },
            new Project { ProjectId = 2, Name = "Assignment 2", Description = "COMP2139 Assignment 2" }
        );
    }
}

// Anytime you make a change to the structure of the database, we need a new migration



























