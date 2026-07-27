namespace CybsClient.Services.DIServices
{
    public class TransactionModeState : ITransactionModeState
    {
        private TransactionMode _mode = TransactionMode.Standard;

        public TransactionMode Mode
        {
            get => _mode;
            private set
            {
                _mode = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void SetMode(TransactionMode mode) => Mode = mode;
    }
}
