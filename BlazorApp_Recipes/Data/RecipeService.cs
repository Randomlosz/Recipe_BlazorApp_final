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

        private static readonly Dictionary<string, double> unitRatioDry = new()
        {
            { "mg", 0.001 },       // 1 mg = 0.001 g
            { "g", 1 },            // base unit
            { "dkg", 10 },         // 1 dkg = 10 g
            { "kg", 1000 },        // 1 kg = 1000 g
            { "ounce(s)", 28 },    // 1 ounce = 28 g
            { "pound(s)", 454 }    // 1 pound = 454 g
        };
        private static readonly Dictionary<string, double> unitRatioLiquid = new()
        {
            { "ml", 1 },               // base unit
            { "cl", 10 },              // 1 cl = 10 ml
            { "dl", 100 },             // 1 dl = 100 ml
            { "l", 1000 },             // 1 l = 1000 ml
            { "csp", 2 },              // 1 csp = 2 ml
            { "tsp", 5 },              // 1 tsp = 5 ml
            { "tbsp", 15 },            // 1 tbsp = 15 ml
            { "cup(s) of", 237 },      // 1 cup = 237 ml
            { "fluid ounce(s)", 30 },  // 1 fl oz = 30 ml
            { "pint", 474 },           // 1 pint = 474 ml
            { "quart", 948 },          // 1 quart = 948 ml
            { "gallon", 3792 }         // 1 gallon = 3792 ml
        };
        private static string[] arrayUnitDry = unitRatioDry.Select(it => it.Key).ToArray(); 
        private static string[] arrayUnitLiquid = unitRatioLiquid.Select(it => it.Key).ToArray();
        private static string[] arrayUnitOther = { "piece(s) of", "slice(s) of", "clove(s) of", "bulb(s) of", "can(s) of", "package(s) of", "stick(s) of", "pinch(es)", "dash(es)", "drop(s)" };
        private static readonly List<string> _unitTypes = arrayUnitDry.Concat(arrayUnitLiquid).Concat(arrayUnitOther).ToList();


        public List<string> CategoryTypes => _categoryTypes;
        public List<string> UnitTypes => _unitTypes;

        public double? RecalculateUnit(double inputValue, string inputUnit, string outputUnit) 
        {
            if (arrayUnitDry.Contains(inputUnit) && arrayUnitDry.Contains(outputUnit))           
                return inputValue * unitRatioDry[inputUnit] / unitRatioDry[outputUnit];
            
            if (arrayUnitLiquid.Contains(inputUnit) && arrayUnitLiquid.Contains(outputUnit))
                return inputValue * unitRatioLiquid[inputUnit] / unitRatioLiquid[outputUnit];

            return null; //recalculation was not possible            
        }

        public void CopyRecipeData2(Recipe source, Recipe target)
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

                targetIngredient.Id= sourceIngredient.Id;
                targetIngredient.Value = sourceIngredient.Value;
                targetIngredient.Unit = sourceIngredient.Unit;
                targetIngredient.Type = sourceIngredient.Type;
            }
        }

        public void CopyRecipeData(Recipe source, Recipe target)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Category = source.Category;
            target.Note = source.Note;
            target.Servings = source.Servings;

            target.Ingredients = source.Ingredients
                .Select(i => new Ingredient
                {
                    Id = i.Id,
                    Value = i.Value,
                    Unit = i.Unit,
                    Type = i.Type
                })
                .ToList();
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
            var initial = await _context.Recipes //server-side filtering for everything else except ingredient
                .Include(r => r.Ingredients)
                .Where(item =>
                    (string.IsNullOrEmpty(SearchTerm_Name) || item.Name.Contains(SearchTerm_Name, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(SearchTerm_Note) || (item.Note != null && item.Note.Contains(SearchTerm_Note, StringComparison.OrdinalIgnoreCase))) &&
                    (IsAllServings || (IsMinServings && item.Servings != null && item.Servings >= ServingsInput) || (IsEqualServings && item.Servings != null && item.Servings == ServingsInput) || (IsMaxServings && item.Servings != null && item.Servings <= ServingsInput)) &&
                    (item.Category != null && categoryList.Contains(item.Category))
                )
                .ToListAsync();

            return initial //client-side filtering for ingredient
            .Where(item =>
                    IsAllIngredient ||
                    (IsMinIngredient   && item.Ingredients.Any(ingredient => RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) != null && ingredient.Value >= RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) && ingredient.Type == IngredientInput_Type)) ||
                    (IsEqualIngredient && item.Ingredients.Any(ingredient => RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) != null && ingredient.Value == RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) && ingredient.Type == IngredientInput_Type)) ||
                    (IsMaxIngredient   && item.Ingredients.Any(ingredient => RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) != null && ingredient.Value <= RecalculateUnit(IngredientInput_Value, IngredientInput_Unit, ingredient.Unit) && ingredient.Type == IngredientInput_Type))
            )
            .ToList();
                
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

        public async Task UpdateRecipeAsync2(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRecipeAsync(Recipe updatedRecipe)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == updatedRecipe.Id);

            recipe.Name = updatedRecipe.Name;
            recipe.Category = updatedRecipe.Category;
            recipe.Note = updatedRecipe.Note;
            recipe.Servings = updatedRecipe.Servings;

            recipe.Ingredients.Clear();

            foreach (var ingredient in updatedRecipe.Ingredients)
            {
                Ingredient ingredientToAdd;

                if (ingredient.Id == 0)
                {
                    // Create a NEW Ingredient instance so EF will insert it
                    ingredientToAdd = new Ingredient
                    {
                        Value = ingredient.Value,
                        Unit = ingredient.Unit,
                        Type = ingredient.Type
                    };

                    _context.Ingredients.Add(ingredientToAdd);
                }
                else
                {
                    // Existing ingredient
                    ingredientToAdd = await _context.Ingredients.FindAsync(ingredient.Id)
                                     ?? throw new Exception("Ingredient not found");

                    _context.Attach(ingredientToAdd);
                }

                recipe.Ingredients.Add(ingredientToAdd);
            }

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
