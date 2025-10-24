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
        private static readonly List<string> _unitTypes = new List<string>
        {
            "mg", "g", "dkg", "kg", "ounce(s)", "pound(s)",
            "ml", "cl", "dl", "l", 
            "csp", "tsp", "tbsp","cup(s) of", "fluid ounce(s)", "pint","quart","gallon",
            "piece(s) of","slice(s) of","clove(s) of", "bulb(s) of","can(s) of","package(s) of", "stick(s) of", "pinch(es)", "dash(es)", "drop(s)"
        };


        public List<string> CategoryTypes => _categoryTypes;
        public List<string> UnitTypes => _unitTypes;
        public List<string> GetIngredientTypes() => _ingredientTypes;
        public void SetIngredientTypes(string value)
        {
                if (!string.IsNullOrWhiteSpace(value.ToString()))
                {
                    _ingredientTypes.Add(value.ToString());
                }
        }

        public void CopyRecipeData(Recipe source, Recipe target)
        {
            target.Name = source.Name;
            target.Category = source.Category;
            target.Note = source.Note;
            target.Servings = source.Servings;
            if (source.Ingredients.Count != target.Ingredients.Count)
                target.Ingredients = Enumerable
                    .Range(0, source.Ingredients.Count)
                    .Select(_ => new Ingredient())
                    .ToList();

            if (source.Ingredients.Count != target.Ingredients.Count) {
                throw new InvalidOperationException("Source and target must have the same number of ingredients.");
            }

            for (int i = 0; i < source.Ingredients.Count; i++)
            {
                var sourceIngredient = source.Ingredients[i];
                var targetIngredient = target.Ingredients[i];

                targetIngredient.Value = sourceIngredient.Value;
                targetIngredient.Unit = sourceIngredient.Unit;
                targetIngredient.Type = sourceIngredient.Type;
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
        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Recipe>> GetFilteredRecipesAsync(string SearchTerm_Name, string SearchTerm_Note, List<string> categoryList,
            bool IsAllServings, bool IsMinServings, bool IsEqualServings, bool IsMaxServings, int ServingsInput,
            bool IsAllIngredient, bool IsMinIngredient, bool IsEqualIngredient, bool IsMaxIngredient, int IngredientInput_Value, string IngredientInput_Unit, string IngredientInput_Type)
        {
            return await _context.Recipes
                .Include(r => r.Ingredients)
                .Where(item =>
                    (string.IsNullOrEmpty(SearchTerm_Name) || item.Name.Contains(SearchTerm_Name, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(SearchTerm_Note) || (item.Note != null && item.Note.Contains(SearchTerm_Note, StringComparison.OrdinalIgnoreCase))) &&
                    (IsAllServings || (IsMinServings && item.Servings != null && item.Servings >= ServingsInput) || (IsEqualServings && item.Servings != null && item.Servings == ServingsInput) || (IsMaxServings && item.Servings != null && item.Servings <= ServingsInput)) &&
                    (item.Category != null && categoryList.Contains(item.Category)) &&
                    (
                        IsAllIngredient ||
                        (IsMinIngredient && item.Ingredients.Any(ingredient => ingredient.Value >= IngredientInput_Value && ingredient.Unit == IngredientInput_Unit && ingredient.Type == IngredientInput_Type)) ||
                        (IsEqualIngredient && item.Ingredients.Any(ingredient => ingredient.Value == IngredientInput_Value && ingredient.Unit == IngredientInput_Unit && ingredient.Type == IngredientInput_Type)) ||
                        (IsMaxIngredient && item.Ingredients.Any(ingredient => ingredient.Value <= IngredientInput_Value && ingredient.Unit == IngredientInput_Unit && ingredient.Type == IngredientInput_Type))
                    )
                )
                .ToListAsync();
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
