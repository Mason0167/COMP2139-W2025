using COMP2139_ICE1.Models;
using Microsoft.EntityFrameworkCore;

namespace COMP2139_ICE1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
       
        
    }
    
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
}



























