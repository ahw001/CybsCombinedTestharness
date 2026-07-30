using CybsClient.Model.DBQueries;

namespace CybsClient.Services.DIServices
{
    public class CartService : ICartService
    {

        private List<DBProduct> cart = new();
        private int totalNumberofItems;
        private decimal? totalPrice = 0;
        public Guid value = Guid.NewGuid();


        public CartService()
        {
            totalNumberofItems = 0;
        }

        public CartService(int initialTotal)
        {

            totalNumberofItems = initialTotal;
        }

        public Guid Value
        {
            get => value;
        }

        public IList<DBProduct> Cart
        {
            get => cart;
        }

        public decimal? Total
        {
            get => totalPrice;
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        decimal? price = 0;
        public void AddProduct(DBProduct product)
        {

            cart.Add(product);
            totalPrice += product.UnitPrice;
            price += product.UnitPrice;
            NotifyStateChanged();
        }
        public void DeleteProduct(DBProduct product)
        {
            cart.Remove(product);
            totalPrice -= product.UnitPrice;
            NotifyStateChanged();
        }

        public void SetTotal(decimal? total)
        {
            totalPrice = total;
            NotifyStateChanged();
        }

        // Must notify like AddProduct/DeleteProduct do: NavMenu's cart badge is an
        // InteractiveServer component in the static layout, so it survives enhanced navigation
        // and is never re-created after checkout clears the cart. Without this the badge keeps
        // showing the pre-checkout item count on every page, including /store/cart — whose own
        // body already recomputes to $0 because it reads Cart directly.
        public void DeleteAll()
        {
            price = 0; totalPrice = 0;
            cart.Clear();
            value = Guid.NewGuid();
            NotifyStateChanged();
        }

    }
}
