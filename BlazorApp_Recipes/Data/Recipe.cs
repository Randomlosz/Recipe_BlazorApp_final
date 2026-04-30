using Microsoft.CodeAnalysis.Scripting.Hosting;
using Newtonsoft.Json.Linq;

namespace BlazorApp_Recipes.Data
{
    public class Ingredient
    {
        public Ingredient() { Value= 0; Unit = ""; Type = ""; }
        public int Id { get; set; } // Primary key
        public double Value{ get; set; }
        public string Unit{ get; set; }
        public string Type { get; set; }

        // Foreign key reference to Recipe: one-to-many (i.e., a Recipe can have multiple Ingredients)
        public List<Recipe> Recipes { get; set; }
        public List<ShoppingList> ShoppingLists { get; set; }
    }
    public class Recipe
    {
        public Recipe() { Category = "Breakfast/Dinner"; }
        public int Id { get; set; } //Primary key
        public string Name { get; set; }
        public string Category { get; set; }

        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public int? Servings { get; set; }

        public string? Note { get; set; }

        //public string? Description { get; set; }

        public List<string> GetIngredients()
        {
            return Ingredients.Select(i => $"{i.Value} {i.Unit} {i.Type}").ToList();
        }

        // Method to set ingredients from a list of strings
        public void SetIngredients(List<string> ingredientDescriptions)
        {
            Ingredients = ingredientDescriptions.Select(description =>
            {
                var parts = description.Split(' ');
                if (double.TryParse(parts[0], out double value))
                {
                    return new Ingredient
                    {
                        Value = value,
                        Unit = parts[1],
                        Type = string.Join(" ", parts.Skip(2)) // Combine the remaining parts into a single string
                    };
                }
                else
                {
                    throw new ArgumentException($"Invalid format. Description must be in the format 'Value Unit Type'.\n Description:-{description}-");
                }
            }).ToList();
        }

    }

    public class ShoppingList
    {
        public string Name { get; set; }
        public string? Note { get; set; }

        public ShoppingList() { }
        public int Id { get; set; } //Primary key
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public List<string> GetIngredients()
        {
            return Ingredients.Select(i => $"{i.Value} {i.Unit} {i.Type}").ToList();
        }

        // Method to set ingredients from a list of strings
        public void SetIngredients(List<string> ingredientDescriptions)
        {
            Ingredients = ingredientDescriptions.Select(description =>
            {
                var parts = description.Split(' ');
                if (double.TryParse(parts[0], out double value))
                {
                    return new Ingredient
                    {
                        Value = value,
                        Unit = parts[1],
                        Type = string.Join(" ", parts.Skip(2)) // Combine the remaining parts into a single string
                    };
                }
                else
                {
                    throw new ArgumentException($"Invalid format. Description must be in the format 'Value Unit Type'.\n Description:-{description}-");
                }
            }).ToList();
        }

    }
}
