using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBAuthTransResponseServices
    {
        public static Task<DbResult<int>> GetAuthTransResponseCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetAuthTransResponseCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.AuthTransResponses.CountAsync();
            });
        public static async Task<List<AuthTransResponseDto>> GetAuthTransResponses()
        {
            try
            {
                Console.WriteLine("Geting full list of Payment Cards ...");
                using CybsDbContext db = new();
                var authTransResponses = await db.AuthTransResponses.ToListAsync();
                List<AuthTransResponseDto> authTransResponseDtos = AuthTransResponseMapper.Map(authTransResponses)!;
                return authTransResponseDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetAuthTransResponses), ex);
                var authTransResponses = new List<AuthTransResponseDto>();
                AuthTransResponseDto authTransResponseDto = new AuthTransResponseDto();
                authTransResponseDto.Error = DbErrorHandler.BuildError(ex, nameof(GetAuthTransResponses));
                authTransResponses.Add(authTransResponseDto);
                return authTransResponses;
            }

        }

        public static async Task<AuthTransResponseDto?> GetAuthTransResponseByUsingId([FromRoute] int id)
        {
            try
            {
                Console.WriteLine($"Geting Auth Trans Response for: {id}");
                using CybsDbContext db = new();
                Task<AuthTransResponse?> task = db.AuthTransResponses.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.AuthTransResponsesId == id);

                var authTransResponse = await task;
                if (authTransResponse == null)
                {
                    return null;
                }
                return AuthTransResponseMapper.Map(authTransResponse);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetAuthTransResponseByUsingId), ex);
                var authTransResponseDto = new AuthTransResponseDto();
                authTransResponseDto.Error = DbErrorHandler.BuildError(ex, nameof(GetAuthTransResponseByUsingId));
                return authTransResponseDto;
            }
        }
        public static Task<DbResult<int>> CreateAuthTransResponse(AuthTransResponseDto authTransResponseDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateAuthTransResponse), async () =>
            {
                AuthTransResponse authTransResponse = AuthTransResponseMapper.Map(authTransResponseDto)!;
                using CybsDbContext db = new();
                db.AuthTransResponses.Add(authTransResponse);
                return await db.SaveChangesAsync();
            });
    }
}
