using CybsClient.Services.DTOs;
using CybsClient.Services.Utilities;
using Microsoft.JSInterop;

namespace CybsClient.Components.Pages.PaybyLink;

// Code-behind for PayByLinkList.razor (combined harness). Extracted from the inline
// @code block to bypass a Razor source-generator bug in SDK 10.0.101 (Replit's only
// available SDK) — see the note at the top of PayByLinkList.razor.
public partial class PayByLinkList
{
    private List<PayByLinkTransactionDto>? records;
    private bool isLoading = true;
    private int? checkingId;
    private int? copiedId;
    private Dictionary<int, string> rowError = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadRecords();
    }

    private async Task LoadRecords()
    {
        isLoading = true;
        rowError.Clear();
        StateHasChanged();

        try
        {
            ApiResult<List<PayByLinkTransactionDto>> result = await CallMinAPIs.GetAllPayByLinksAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                records = result.Data;
            }
            else
            {
                records = new List<PayByLinkTransactionDto>();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PayByLinkList] LoadRecords failed: {ex.Message}");
            records = new List<PayByLinkTransactionDto>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task CheckStatus(int id)
    {
        checkingId = id;
        rowError.Remove(id);
        StateHasChanged();

        try
        {
            ApiResult<PayByLinkTransactionDto> result = await CallMinAPIs.CheckPayByLinkStatusAsync(id);

            if (!result.IsSuccess || result.Error is not null)
            {
                rowError[id] = result.Error?.Message ?? "Status check failed.";
            }
            else
            {
                // Refresh the full list so all rows update
                await LoadRecords();
            }
        }
        catch (Exception ex)
        {
            rowError[id] = ex.Message;
        }
        finally
        {
            checkingId = null;
            StateHasChanged();
        }
    }

    private async Task CopyLink(int id, string? link)
    {
        if (!string.IsNullOrWhiteSpace(link))
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", link);
            copiedId = id;
            StateHasChanged();
        }
    }

    private static string TruncateLink(string link)
    {
        return link.Length > 40 ? link[..40] + "…" : link;
    }

    // ── Badge helpers ────────────────────────────────────────────────────────

    private static string GetAgeBadgeClass(PayByLinkTransactionDto record)
    {
        if (record.Status?.Equals("PAID", StringComparison.OrdinalIgnoreCase) == true)
            return "bg-success";

        double ageDays = (DateTime.UtcNow - record.CreatedAt).TotalDays;

        return ageDays switch
        {
            < 15 => "bg-success",
            <= 30 => "bg-warning text-dark",
            _ => "bg-danger"
        };
    }

    private static string GetAgeLabel(PayByLinkTransactionDto record)
    {
        if (record.Status?.Equals("PAID", StringComparison.OrdinalIgnoreCase) == true)
            return "Paid";

        double ageDays = (DateTime.UtcNow - record.CreatedAt).TotalDays;

        if (ageDays < 1) return "Today";
        return $"{(int)ageDays}d old";
    }

    private static string GetStatusBadgeClass(string? status) =>
        status?.ToUpperInvariant() switch
        {
            "ACTIVE" => "bg-primary",
            "PAID" => "bg-success",
            "PROCESSING" => "bg-info text-dark",
            "CANCELLED" or "EXPIRED" => "bg-secondary",
            "FAILED" => "bg-danger",
            _ => "bg-light text-dark border"
        };
}
