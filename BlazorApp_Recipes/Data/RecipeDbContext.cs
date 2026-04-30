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

            builder.Entity<Recipe>().HasKey(s => s.Id);
            builder.Entity<Ingredient>().HasKey(s => s.Id);
            builder.Entity<ShoppingList>().HasKey(s => s.Id);

            builder.Entity<Recipe>().ToTable("Recipe");
            builder.Entity<Ingredient>().ToTable("Ingredient");
            builder.Entity<ShoppingList>().ToTable("ShoppingList");

            builder.Entity<Recipe>()
                .HasMany(r => r.Ingredients)
                .WithMany(i => i.Recipes)
                .UsingEntity(j => j.ToTable("RecipeIngredients"));

            builder.Entity<ShoppingList>()
                .HasMany(s => s.Ingredients)
                .WithMany(i => i.ShoppingLists)
                .UsingEntity(j => j.ToTable("ShoppingListIngredients"));
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(Console.WriteLine);
        }
    }
}
