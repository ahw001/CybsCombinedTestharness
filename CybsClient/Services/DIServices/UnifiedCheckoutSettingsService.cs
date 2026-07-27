using CybsClient.Services.DTOs;

namespace CybsClient.Services.DIServices;

public class UnifiedCheckoutSettingsService : IUnifiedCheckoutSettingsService
{
    private UnifiedCheckoutConfigurationDto? _selectedConfig;

    public event Action? OnChange;

    public UnifiedCheckoutConfigurationDto? SelectedConfig => _selectedConfig;

    public void SetConfig(UnifiedCheckoutConfigurationDto? config)
    {
        _selectedConfig = config;
        OnChange?.Invoke();
    }
}
