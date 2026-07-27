using System.Text.Json;
using CybsClient.Model.CyberStore;
using CybsClient.Services.Utilities;

namespace CybsClient.Services.DIServices;

public class ElectronicProductService : IElectronicProductService
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<ElectronicProduct>> GetAllAsync()
    {
        var client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
        var requestUrl = $"{client.BaseAddress}api/ElectronicProduct";
        Console.WriteLine($"[ElectronicProductService] GET {requestUrl}");

        var response = await client.GetAsync("api/ElectronicProduct");
        Console.WriteLine($"[ElectronicProductService] Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[ElectronicProductService] Body length: {rawJson.Length} chars");
        Console.WriteLine($"[ElectronicProductService] Body (first 1000): {rawJson[..Math.Min(rawJson.Length, 1000)]}");

        var products = JsonSerializer.Deserialize<List<ElectronicProduct>>(rawJson, _jsonOptions);
        Console.WriteLine($"[ElectronicProductService] Deserialized {products?.Count ?? 0} product(s)");

        if (products is { Count: > 0 })
        {
            var first = products[0];
            Console.WriteLine($"[ElectronicProductService] Sample → Id={first.ElectronicProductId}, Name={first.ProductName}, Label={first.ProductLabel}, Price={first.UnitPrice}, ProductLineId={first.ProductLineId}, Picture={(first.Picture is null ? "null" : $"{first.Picture.Length} chars")}");
        }

        return products ?? [];
    }

    public async Task<List<ElectronicProduct>> GetByKeywordAsync(string keyword)
    {
        var client = HttpClientHelper.CreateClient("Cybs.WebApi.Service");
        // Pass the keyword as a query parameter; if the API ignores it the client-side
        // filter below acts as a fallback.
        var relativeUrl = $"api/ElectronicProduct?productName={Uri.EscapeDataString(keyword)}";
        Console.WriteLine($"[ElectronicProductService] GET {client.BaseAddress}{relativeUrl}");

        var response = await client.GetAsync(relativeUrl);
        Console.WriteLine($"[ElectronicProductService] Response: {(int)response.StatusCode} {response.StatusCode}");

        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ElectronicProduct>>(rawJson, _jsonOptions) ?? [];

        // Client-side filter ensures only matching products are returned even if the
        // API does not support the productName query parameter.
        var filtered = products
            .Where(ep => ep.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine($"[ElectronicProductService] GetByKeyword '{keyword}': {filtered.Count}/{products.Count} matched");

        return filtered;
    }
}
