using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BlazorApp_Recipes.Data
{
    public class RecipeDbContext : DbContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options): base(options){ }

        public DbSet<Recipe> Recipes{ get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Recipe>().HasKey(s=>s.Id);
            builder.Entity<Ingredient>().HasKey(s=>s.Id);

            builder.Entity<Recipe>()
                .HasMany(r => r.Ingredients)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId);

            builder.Entity<Recipe>().ToTable("Recipe");
            builder.Entity<Ingredient>().ToTable("Ingredient");

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(Console.WriteLine);
        }
    }
}
