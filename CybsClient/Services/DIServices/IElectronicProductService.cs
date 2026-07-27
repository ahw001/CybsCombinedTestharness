using CybsClient.Model.CyberStore;

namespace CybsClient.Services.DIServices;

public interface IElectronicProductService
{
    Task<List<ElectronicProduct>> GetAllAsync();
    Task<List<ElectronicProduct>> GetByKeywordAsync(string keyword);
}
