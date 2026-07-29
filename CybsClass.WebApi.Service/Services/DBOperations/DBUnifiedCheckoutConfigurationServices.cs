using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBUnifiedCheckoutConfigurationServices
{
    // ── Create ──────────────────────────────────────────────────────────────
    public static Task<DbResult<UnifiedCheckoutConfiguration?>> CreateAsync(UnifiedCheckoutConfigurationDto dto) =>
        DbErrorHandler.GuardAsync<UnifiedCheckoutConfiguration?>(nameof(CreateAsync), async () =>
        {
            var entity = MapFromDto(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            using CybsDbContext db = new();
            db.UnifiedCheckoutConfiguration.Add(entity);
            await db.SaveChangesAsync();

            return entity;
        });

    // ── Read All ─────────────────────────────────────────────────────────────
    public static Task<DbResult<List<UnifiedCheckoutConfiguration>>> GetAllAsync() =>
        DbErrorHandler.GuardAsync(nameof(GetAllAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.UnifiedCheckoutConfiguration
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        });

    // ── Read by ID ────────────────────────────────────────────────────────────
    // NOTE: deliberately still returns a bare nullable entity (not DbResult<T>) — it is consumed
    // by the capture-context builders (CallForCaptureContextV1 / CallForCaptureContextV0FromConfig),
    // which treat a missing config as "fall back to defaults". Changing this signature would
    // ripple outside the CRUD surface, so it keeps its sentinel-on-failure behaviour.
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
            DbErrorHandler.Log(nameof(GetByIdAsync), ex);
            return null;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public static Task<DbResult<UnifiedCheckoutConfiguration?>> UpdateAsync(int id, UnifiedCheckoutConfigurationDto dto) =>
        DbErrorHandler.GuardAsync<UnifiedCheckoutConfiguration?>(nameof(UpdateAsync), async () =>
        {
            using CybsDbContext db = new();
            var entity = await db.UnifiedCheckoutConfiguration
                .FirstOrDefaultAsync(c => c.UnifiedCheckoutConfigurationId == id);

            if (entity is null) return null;

            ApplyDtoToEntity(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return entity;
        });

    // ── Upsert (create if Id == 0, else update) ──────────────────────────────
    public static Task<DbResult<UnifiedCheckoutConfiguration?>> UpsertAsync(UnifiedCheckoutConfigurationDto dto)
    {
        return dto.UnifiedCheckoutConfigurationId == 0
            ? CreateAsync(dto)
            : UpdateAsync(dto.UnifiedCheckoutConfigurationId, dto);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public static Task<DbResult<bool>> DeleteAsync(int id) =>
        DbErrorHandler.GuardAsync(nameof(DeleteAsync), async () =>
        {
            using CybsDbContext db = new();
            return await db.UnifiedCheckoutConfiguration
                .Where(c => c.UnifiedCheckoutConfigurationId == id)
                .ExecuteDeleteAsync() > 0;
        });

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
