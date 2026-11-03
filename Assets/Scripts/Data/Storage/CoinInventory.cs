namespace Data.Storage
{
    public class CoinInventory
    {
        private const string CoinKey = "Coins";
        private const int InitialBalance = 2000;
        private readonly CachedInt _balance = CachedInt.Get(CoinKey, InitialBalance);

        public int Balance => _balance.Value;

        public void Add(int amount)
        {
            _balance.Value += amount;
            CachedInt.Save();
        }

        public bool TrySpend(int amount)
        {
            if (_balance.Value < amount)
            {
                return false;
            }
            _balance.Value -= amount;
            CachedInt.Save();
            return true;
        }
    }
}
