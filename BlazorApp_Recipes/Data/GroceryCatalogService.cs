namespace BlazorApp_Recipes.Data
{
    public class GroceryCatalogService
    {
        public Dictionary<string, List<string>> GroceryCatalog { get; private set; }
        public GroceryCatalogService() { GroceryCatalog = LoadGroceryCatalog(); }
        private Dictionary<string, List<string>> LoadGroceryCatalog() 
        {
            return File.ReadLines("Data/dataset_with_700_elements.csv").Skip(1)
                .Select(line => line.Split(','))
                .GroupBy(parts => parts[0])
                .ToDictionary(g => g.Key, g => g.Select(parts => parts[1])
                .ToList()); 
        }
    }
}
