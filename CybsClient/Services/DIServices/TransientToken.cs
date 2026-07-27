namespace CybsClient.Services.DIServices
{
    public class TransientToken : ITransientToken
    {
        private readonly Guid _id = Guid.NewGuid();
        private string? _tt;
        public event Action? OnChange;

        public Guid Id => _id;
        public string? Tt
        {
            get => _tt;
            private set
            {
                _tt = value;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void SetTt(string? value) => Tt = value;
        public void DeleteTt() => Tt = null;

    }
}
