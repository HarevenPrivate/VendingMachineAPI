namespace VendingMachineAPI.Interface
{
    public interface IProductRepository
    {
        bool IsExist(string productId);
        (decimal price, string description) GetProduct(string productId);

    }
}
