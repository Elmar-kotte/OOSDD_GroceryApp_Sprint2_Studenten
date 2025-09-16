using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            List<Product> allProducts = _productService.GetAll();
            List<GroceryListItem> itemsInGroceryList = _groceryListItemsService.GetAllOnGroceryListId(groceryList.Id);
            
            AvailableProducts.Clear();
            
            foreach (var product in allProducts)
            {
                bool productInGroceryList = itemsInGroceryList.Any(p => p.ProductId == product.Id);
                bool stockEmpty = product.Stock <= 0;

                if (productInGroceryList || stockEmpty)
                    continue;
                
                AvailableProducts.Add(product);
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null || product.Id <= 0)
            {
                Console.WriteLine($"Null product passed to " + nameof(AddProduct));
                return;
            }

            GroceryListItem newItem = new GroceryListItem(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(newItem);
            
            product.Stock--;
            
            AvailableProducts.Remove(product);
            OnGroceryListChanged(GroceryList);
        }
    }
}
