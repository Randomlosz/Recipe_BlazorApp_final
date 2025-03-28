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
        //------------- Service function -----------------
        //------------------------------------------------

        public async Task<Recipe[]> GetRecipeAsync(string name) => await Task.FromResult(_recipes.Where(x => x.Name == name).ToArray());
        public async Task<Recipe[]> GetAllRecipeAsync() => await Task.FromResult(_recipes.ToArray());

        public async Task AddRecipeAsync(string name, string category, List<Ingredient> ingredients, int? serving, string? note, string? description) 
        {   
            Recipe newRecipe = new Recipe();
            newRecipe.Name = name;
            newRecipe.Category = category;
            newRecipe.Ingredients = ingredients;
            newRecipe.Servings = serving;
            newRecipe.Note = note;
            //now.Description = description;

            await Task.Run(  () => _recipes.Add(newRecipe)  );
        }
        public async Task AddRecipeAsync(string name, string category, List<Ingredient> ingredients)
        {
            await AddRecipeAsync(name, category, ingredients, null, null, null);
        }
        public async Task AddRecipeAsync(Recipe newRecipe)
        {
            await Task.Run(() => _recipes.Add(newRecipe));
        }



    }
}
