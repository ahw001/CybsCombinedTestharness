using Microsoft.EntityFrameworkCore;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.EntityModels;

namespace CybsClass.WebApi.Service.Services.DBOperations;

public static class DBBoardingPortfolioServices
{
    // ── Create ──────────────────────────────────────────────────────────────
    public static async Task<BoardingPortfolio?> CreateAsync(BoardingPortfolioDto dto)
    {
        try
        {
            var entity = MapFromDto(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            using CybsDbContext db = new();
            db.BoardingPortfolios.Add(entity);
            await db.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBBoardingPortfolio] Create error: {ex}");
            return null;
        }
    }

    // ── Read All ─────────────────────────────────────────────────────────────
    public static async Task<List<BoardingPortfolio>> GetAllAsync()
    {
        try
        {
            using CybsDbContext db = new();
            return await db.BoardingPortfolios
                .AsNoTracking()
                .OrderBy(p => p.PortfolioName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBBoardingPortfolio] GetAll error: {ex}");
            return new List<BoardingPortfolio>();
        }
    }

    // ── Read by ID ────────────────────────────────────────────────────────────
    public static async Task<BoardingPortfolio?> GetByIdAsync(int id)
    {
        try
        {
            using CybsDbContext db = new();
            return await db.BoardingPortfolios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.BoardingPortfolioId == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBBoardingPortfolio] GetById error: {ex}");
            return null;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public static async Task<BoardingPortfolio?> UpdateAsync(int id, BoardingPortfolioDto dto)
    {
        try
        {
            using CybsDbContext db = new();
            var entity = await db.BoardingPortfolios
                .FirstOrDefaultAsync(p => p.BoardingPortfolioId == id);

            if (entity is null) return null;

            ApplyDtoToEntity(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBBoardingPortfolio] Update error: {ex}");
            return null;
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    public static async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using CybsDbContext db = new();
            return await db.BoardingPortfolios
                .Where(p => p.BoardingPortfolioId == id)
                .ExecuteDeleteAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[DBBoardingPortfolio] Delete error: {ex}");
            return false;
        }
    }

    // ── Map helpers ───────────────────────────────────────────────────────────
    private static BoardingPortfolio MapFromDto(BoardingPortfolioDto dto)
    {
        var entity = new BoardingPortfolio();
        ApplyDtoToEntity(dto, entity);
        return entity;
    }

    private static void ApplyDtoToEntity(BoardingPortfolioDto dto, BoardingPortfolio entity)
    {
        entity.PortfolioName                  = dto.PortfolioName ?? string.Empty;
        entity.Description                    = dto.Description;
        entity.ExpectedSignatureMerchantId    = dto.ExpectedSignatureMerchantId;
        entity.BoardingPackageId              = dto.BoardingPackageId;
        entity.CardProcessingTemplateId       = dto.CardProcessingTemplateId;
        entity.VirtualTerminalTemplateId      = dto.VirtualTerminalTemplateId;
        entity.TokenManagementTemplateId      = dto.TokenManagementTemplateId;
        entity.PayerAuthenticationTemplateId  = dto.PayerAuthenticationTemplateId;
    }

    // ── DTO projection ────────────────────────────────────────────────────────
    public static BoardingPortfolioDto ToDto(BoardingPortfolio entity)
    {
        return new BoardingPortfolioDto
        {
            BoardingPortfolioId          = entity.BoardingPortfolioId,
            PortfolioName                = entity.PortfolioName,
            Description                  = entity.Description,
            ExpectedSignatureMerchantId  = entity.ExpectedSignatureMerchantId,
            BoardingPackageId            = entity.BoardingPackageId,
            CardProcessingTemplateId     = entity.CardProcessingTemplateId,
            VirtualTerminalTemplateId    = entity.VirtualTerminalTemplateId,
            TokenManagementTemplateId    = entity.TokenManagementTemplateId,
            PayerAuthenticationTemplateId = entity.PayerAuthenticationTemplateId
        };
    }
}
