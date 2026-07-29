using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBFollowOnTransResponseServices
    {
        public static Task<DbResult<int>> GetFollowOnTransResponseCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetFollowOnTransResponseCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.FollowOnTransResponses.CountAsync();
            });
        public static async Task<List<FollowOnTransResponseDto>> GetFollowOnTransResponses()
        {
            try
            {
                Console.WriteLine("Geting full list of Follow On Transactions ...");
                using CybsDbContext db = new();
                var followOnTransResponses = await db.FollowOnTransResponses.ToListAsync();
                List<FollowOnTransResponseDto> FollowOnTransResponseDtos = FollowOnTransResponseMapper.Map(followOnTransResponses)!;
                return FollowOnTransResponseDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetFollowOnTransResponses), ex);
                var followOnTransResponses = new List<FollowOnTransResponseDto>();
                FollowOnTransResponseDto followOnTransResponseDto = new FollowOnTransResponseDto();
                followOnTransResponseDto.Error = DbErrorHandler.BuildError(ex, nameof(GetFollowOnTransResponses));
                followOnTransResponses.Add(followOnTransResponseDto);
                return followOnTransResponses;
            }

        }

        public static async Task<FollowOnTransResponseDto?> GetFollowOnTransResponseByUsingId([FromRoute] int id)
        {
            try
            {
                Console.WriteLine($"Geting Follow On Transactions for: {id}");
                using CybsDbContext db = new();
                Task<FollowOnTransResponse?> task = db.FollowOnTransResponses.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.TransResponseId == id);

                var followOnTransResponse = await task;
                if (followOnTransResponse == null)
                {
                    return null;
                }
                return FollowOnTransResponseMapper.Map(followOnTransResponse);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetFollowOnTransResponseByUsingId), ex);
                var followOnTransResponseDto = new FollowOnTransResponseDto
                {
                    Error = DbErrorHandler.BuildError(ex, nameof(GetFollowOnTransResponseByUsingId))
                };
                return followOnTransResponseDto;
            }
        }
        public static Task<DbResult<int>> CreateFollowOnTransResponse(FollowOnTransResponseDto followOnTransResponseDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateFollowOnTransResponse), async () =>
            {
                FollowOnTransResponse followOnTransResponse = FollowOnTransResponseMapper.Map(followOnTransResponseDto)!;
                using CybsDbContext db = new();
                db.FollowOnTransResponses.Add(followOnTransResponse);
                return await db.SaveChangesAsync();
            });
    }
}
