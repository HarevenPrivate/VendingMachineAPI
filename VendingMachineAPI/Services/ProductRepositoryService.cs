using VendingMachineAPI.Interface;
namespace VendingMachineAPI.Services
{
    public class ProductRepositoryService : IProductRepository
    {
        private readonly Dictionary<string, (decimal price, string description)> _catalog = new()
        {
            ["A1"] = (2.50m,"Kola"),
            ["A2"] = (1.75m,"Kinly"),
            ["B1"] = (3.00m,"Sprit"),
            ["B2"] = (2.00m,"Egozy")
        };
        public (decimal price, string description) GetProduct(string productId)
        {
            var normalized = Normalize(productId);
            if (_catalog.TryGetValue(normalized, out var product))
            {
                return product;
            }

            throw new KeyNotFoundException($"Product '{productId}' does not exist.");
        }

        public bool IsExist(string productId)
        {
            var normalized = Normalize(productId);
            return _catalog.ContainsKey(normalized);
        }

        private static string Normalize(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return string.Empty;

            var id = productId.Trim().ToUpperInvariant();

            // Handle both "A1" and "1A"
            if (id.Length == 2)
            {
                if (char.IsLetter(id[0]) && char.IsDigit(id[1]))
                    return id;
                if (char.IsDigit(id[0]) && char.IsLetter(id[1]))
                    return $"{id[1]}{id[0]}"; // swap
            }

            return id;
        }
    }
}
