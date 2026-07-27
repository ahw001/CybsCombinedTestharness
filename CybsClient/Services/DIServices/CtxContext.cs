using CybsClient.Services.DIServices;

public class CtxContext : ICtxContext
{
    private readonly Guid _id = Guid.NewGuid();
    private string? _ctx;
    public event Action? OnChange;

    public Guid Id => _id;
    public string? Ctx
    {
        get => _ctx;
        private set
        {
            _ctx = value;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void SetCtx(string? value) => Ctx = value;
    public void DeleteCtx() => Ctx = null;
}
