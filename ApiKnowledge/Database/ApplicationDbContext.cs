using Microsoft.EntityFrameworkCore;

namespace ApiKnowledge.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    

}