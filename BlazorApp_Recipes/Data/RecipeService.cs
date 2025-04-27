using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace BlazorApp_Recipes.Data
{
    public class RecipeService
    {
        List<Recipe> _recipes = new List<Recipe>();


        private static readonly List<string> _categoryTypes = new List<string>
        {
            "Breakfast/Dinner", "Main Course", "Side-dish", "Dessert", "Baked-goods", "Soup", "Turmix", "Holiday", "Snack", "Other"
        };
        private static List<string> _ingredientTypes = new List<string>
        {
            "milk", "water", "butter", "salt", "sugar", "powdered sugar", "baking-soda", "yeast", "yoghurt", "sour cream", "flour", "orange", "banana", "apple", "meat", "olive oil", "oil"
        };
        
        public List<string> CategoryTypes => _categoryTypes;
        public List<string> GetIngredientTypes() => _ingredientTypes;
        public void SetIngredientTypes(string value)
        {
                if (!string.IsNullOrWhiteSpace(value.ToString()))
                {
                    _ingredientTypes.Add(value.ToString());
                }
        }


        //------------------------------------------------
        //------------- Database function -----------------
        //------------------------------------------------

        private readonly RecipeDbContext _context;

        public RecipeService(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes.Include(r => r.Ingredients).ToListAsync();
        }

        public async Task AddRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
        }

        //var tables = await context.Database.ExecuteSqlRawAsync("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
    }
}
