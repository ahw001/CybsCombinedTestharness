using CybsClient.Services.DTOs;

namespace CybsClient.Services.DIServices;

// Holds the Unified Checkout configuration selected on /store/checkout-config for the rest of
// the user's session (Phase 0 / Execution Goal 1, UnifiedCheckoutPlanning.md). Scoped per
// Blazor Server circuit, same lifetime pattern as ICtxContext/ITransientToken.
public interface IUnifiedCheckoutSettingsService
{
    UnifiedCheckoutConfigurationDto? SelectedConfig { get; }

    event Action? OnChange;

    void SetConfig(UnifiedCheckoutConfigurationDto? config);
}
