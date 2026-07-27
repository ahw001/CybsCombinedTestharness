
public interface ICtxContext
{
    string? Ctx { get; }
    Guid Id { get; }

    event Action? OnChange;

    void DeleteCtx();
    void SetCtx(string? value);
}