using Microsoft.EntityFrameworkCore;

namespace BlazorApp_Recipes.Data
{
    public class ShoppingListService
    {
        List<ShoppingList> _shoppinglists = new List<ShoppingList>();
        
        private readonly RecipeDbContext _context;

        public ShoppingListService(RecipeDbContext context)
        {
            _context = context;
        }
        public void CopyShoppingListData(ShoppingList source, ShoppingList target)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.Note = source.Note;

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


        public async Task<List<ShoppingList>> GetAllShoppingListsAsync()
        {
            return await _context.ShoppingLists.Include(r => r.Ingredients).ToListAsync();
        }
        public async Task<ShoppingList?> GetShoppingListByIdAsync(int id)
        {
            return await _context.ShoppingLists.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task UpdateShoppingListAsync(ShoppingList updatedShoppingList)
        {
            var shoppinglist = await _context.ShoppingLists
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == updatedShoppingList.Id);

            shoppinglist.Name = updatedShoppingList.Name;
            shoppinglist.Note = updatedShoppingList.Note;

            var oldIngredients = shoppinglist.Ingredients.ToList();

            shoppinglist.Ingredients.Clear();

            foreach (var ingredient in updatedShoppingList.Ingredients)
            {
                Ingredient ingredientToAdd;

                if (ingredient.Id == 0)
                {
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
                    ingredientToAdd = await _context.Ingredients.FindAsync(ingredient.Id)
                                        ?? throw new Exception("Ingredient not found");

                    _context.Attach(ingredientToAdd);
                }

                shoppinglist.Ingredients.Add(ingredientToAdd);
            }

            // DELETE ORPHANED INGREDIENTS
            foreach (var old in oldIngredients)
            {
                bool usedInCurrentShoppingList = updatedShoppingList.Ingredients.Any(i => i.Id == old.Id);
                bool usedInRecipes = await _context.Recipes.AnyAsync(r => r.Ingredients.Any(i => i.Id == old.Id));

                bool stillUsed = usedInCurrentShoppingList || usedInRecipes;


                if (!stillUsed)
                {
                    _context.Ingredients.Remove(old);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task DeleteShoppingListAsync(int id)
        {
            var shoppinglist = await _context.ShoppingLists.FindAsync(id);
            if (shoppinglist != null)
            {
                _context.ShoppingLists.Remove(shoppinglist);
                await _context.SaveChangesAsync();
            }
        }


    }
}
