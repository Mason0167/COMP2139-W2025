using System.ComponentModel.DataAnnotations;

namespace COMP2139_ICE1.Models;

public class ProjectTask
{
    [Key]
    public int ProjectTaskId { get; set; }
    
    [Required]
    // Only for ASP.NET
    public required string Title { get; set; }
    
    [Required]
    public required string Description { get; set; }
    
    public int  ProjectId { get; set; }
    
    public Project? Project { get; set; }
}