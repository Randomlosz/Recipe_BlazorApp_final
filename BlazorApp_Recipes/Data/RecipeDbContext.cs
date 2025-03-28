using Microsoft.EntityFrameworkCore;

namespace BlazorApp_Recipes.Data
{
    public class RecipeDbContext : DbContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options): base(options)
        {
        }

        public DbSet<Recipe> Recipe{ get; set; }
    }
}
