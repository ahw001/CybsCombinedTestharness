using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBUnifiedCheckoutConfigurationServices
{
    // ── Create ──────────────────────────────────────────────────────────────
    public static async Task<UnifiedCheckoutConfiguration?> CreateAsync(UnifiedCheckoutConfigurationDto dto)
    {
        try
        {
            var entity = MapFromDto(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            using CybsDbContext db = new();
            db.UnifiedCheckoutConfiguration.Add(entity);
            await db.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBUnifiedCheckoutConfiguration] Create error: {ex}");
            return null;
        }
    }

    // ── Read All ─────────────────────────────────────────────────────────────
    public static async Task<List<UnifiedCheckoutConfiguration>> GetAllAsync()
    {
        try
        {
            using CybsDbContext db = new();
            return await db.UnifiedCheckoutConfiguration
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBUnifiedCheckoutConfiguration] GetAll error: {ex}");
            return new List<UnifiedCheckoutConfiguration>();
        }
    }

    // ── Read by ID ────────────────────────────────────────────────────────────
    public static async Task<UnifiedCheckoutConfiguration?> GetByIdAsync(int id)
    {
        try
        {
            using CybsDbContext db = new();
            return await db.UnifiedCheckoutConfiguration
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UnifiedCheckoutConfigurationId == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBUnifiedCheckoutConfiguration] GetById error: {ex}");
            return null;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public static async Task<UnifiedCheckoutConfiguration?> UpdateAsync(int id, UnifiedCheckoutConfigurationDto dto)
    {
        try
        {
            using CybsDbContext db = new();
            var entity = await db.UnifiedCheckoutConfiguration
                .FirstOrDefaultAsync(c => c.UnifiedCheckoutConfigurationId == id);

            if (entity is null) return null;

            ApplyDtoToEntity(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBUnifiedCheckoutConfiguration] Update error: {ex}");
            return null;
        }
    }

    // ── Upsert (create if Id == 0, else update) ──────────────────────────────
    public static async Task<UnifiedCheckoutConfiguration?> UpsertAsync(UnifiedCheckoutConfigurationDto dto)
    {
        return dto.UnifiedCheckoutConfigurationId == 0
            ? await CreateAsync(dto)
            : await UpdateAsync(dto.UnifiedCheckoutConfigurationId, dto);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public static async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using CybsDbContext db = new();
            return await db.UnifiedCheckoutConfiguration
                .Where(c => c.UnifiedCheckoutConfigurationId == id)
                .ExecuteDeleteAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBUnifiedCheckoutConfiguration] Delete error: {ex}");
            return false;
        }
    }

    // ── Map helpers ───────────────────────────────────────────────────────────
    private static UnifiedCheckoutConfiguration MapFromDto(UnifiedCheckoutConfigurationDto dto)
    {
        var entity = new UnifiedCheckoutConfiguration();
        ApplyDtoToEntity(dto, entity);
        return entity;
    }

    private static void ApplyDtoToEntity(UnifiedCheckoutConfigurationDto dto, UnifiedCheckoutConfiguration entity)
    {
        entity.Name                     = dto.Name ?? string.Empty;
        entity.Description              = dto.Description;
        entity.AllowedPaymentTypes       = dto.AllowedPaymentTypes;
        entity.AllowedCardNetworks       = dto.AllowedCardNetworks;
        entity.Country                   = dto.Country;
        entity.Locale                    = dto.Locale;
        entity.ButtonType                = dto.ButtonType;
        entity.BillingType               = dto.BillingType;
        entity.RequestShipping           = dto.RequestShipping;
        entity.RequestEmail              = dto.RequestEmail;
        entity.RequestPhone              = dto.RequestPhone;
        entity.RequestSaveCredentials    = dto.RequestSaveCredentials;
        entity.ShowConfirmationStep      = dto.ShowConfirmationStep;
        entity.ShowAcceptedNetworkIcons  = dto.ShowAcceptedNetworkIcons;
        entity.CompleteMandateType       = dto.CompleteMandateType;
        entity.EnableTms                 = dto.EnableTms;
        entity.TmsTokenTypes             = dto.TmsTokenTypes;
        entity.Enable3ds                 = dto.Enable3ds;
        entity.EnableDecisionManager     = dto.EnableDecisionManager;
    }

    // ── DTO projection ────────────────────────────────────────────────────────
    public static UnifiedCheckoutConfigurationDto ToDto(UnifiedCheckoutConfiguration entity)
    {
        return new UnifiedCheckoutConfigurationDto
        {
            UnifiedCheckoutConfigurationId = entity.UnifiedCheckoutConfigurationId,
            Name                           = entity.Name,
            Description                    = entity.Description,
            AllowedPaymentTypes            = entity.AllowedPaymentTypes,
            AllowedCardNetworks            = entity.AllowedCardNetworks,
            Country                        = entity.Country,
            Locale                         = entity.Locale,
            ButtonType                     = entity.ButtonType,
            BillingType                    = entity.BillingType,
            RequestShipping                = entity.RequestShipping,
            RequestEmail                   = entity.RequestEmail,
            RequestPhone                   = entity.RequestPhone,
            RequestSaveCredentials         = entity.RequestSaveCredentials,
            ShowConfirmationStep           = entity.ShowConfirmationStep,
            ShowAcceptedNetworkIcons       = entity.ShowAcceptedNetworkIcons,
            CompleteMandateType            = entity.CompleteMandateType,
            EnableTms                      = entity.EnableTms,
            TmsTokenTypes                  = entity.TmsTokenTypes,
            Enable3ds                      = entity.Enable3ds,
            EnableDecisionManager          = entity.EnableDecisionManager,
            CreatedAt                      = entity.CreatedAt,
            UpdatedAt                      = entity.UpdatedAt
        };
    }
}
