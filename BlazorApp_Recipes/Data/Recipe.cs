using Newtonsoft.Json.Linq;

namespace BlazorApp_Recipes.Data
{
    public class Ingredient
    {
        public int Value{ get; set; }
        public string Unit{ get; set; }
        public string Type { get; set; }
    }
    public class Recipe
    {
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
                if (parts.Length == 3 && int.TryParse(parts[0], out int value))
                {
                    return new Ingredient
                    {
                        Value = value,
                        Unit = parts[1],
                        Type = parts[2]
                    };
                }
                else
                {
                    throw new ArgumentException("Invalid format. Description must be in the format 'Value Unit Type'.");
                }
            }).ToList();
        }

    }
}
