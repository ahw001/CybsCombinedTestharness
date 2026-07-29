using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBNetworkTokenServices
    {
        public static Task<DbResult<int>> GetNetworkTokenInfosCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetNetworkTokenInfosCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.NetworkTokenInfos.CountAsync();
            });
        public static async Task<List<NetworkTokenInfoDto>> GetNetworkTokens()
        {
            try
            {
                Console.WriteLine("Geting full list of Network Tokens ...");
                using CybsDbContext db = new();
                var networkTokenInfos = await db.NetworkTokenInfos.ToListAsync();
                List<NetworkTokenInfoDto> NetworkTokenInfoDtos = NetworkTokenMapper.MapToDtos(networkTokenInfos)!;
                return NetworkTokenInfoDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetNetworkTokens), ex);
                var networkTokenInfos = new List<NetworkTokenInfoDto>();
                NetworkTokenInfoDto NetworkTokenInfoDto = new NetworkTokenInfoDto();
                NetworkTokenInfoDto.Error = ex.ToString();
                networkTokenInfos.Add(NetworkTokenInfoDto);
                return networkTokenInfos;
            }

        }

        public static async Task<List<NetworkTokenInfoDto>> GetNetworkTokenByUsingId(int paymentCardId)
        {
            try 
            { 
                using CybsDbContext db = new();
                var networkInfos = await db.NetworkTokenInfos.Where(n => n.PaymentCardId == paymentCardId).ToListAsync();
                var networkTokenDtos = NetworkTokenMapper.MapToDtos(networkInfos)!;
                return networkTokenDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetNetworkTokenByUsingId), ex);
                var NetworkTokenInfoDtos = new List<NetworkTokenInfoDto>();
                NetworkTokenInfoDto networkTokenInfoDto = new NetworkTokenInfoDto();
                networkTokenInfoDto.Error = ex.Message;
                NetworkTokenInfoDtos.Add(networkTokenInfoDto);
                return NetworkTokenInfoDtos!;
            }
        }

        public static Task<DbResult<int>> CreateNetworkToken(NetworkTokenInfoDto networkTokenInfoDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateNetworkToken), async () =>
            {
                NetworkTokenInfo networkTokenInfo = NetworkTokenMapper.Map(networkTokenInfoDto)!;
                using CybsDbContext db = new();
                db.NetworkTokenInfos.Add(networkTokenInfo);
                return await db.SaveChangesAsync();
            });

        public static Task<DbResult<int>> UpdateNetworkToken(int id, NetworkTokenInfoDto networkTokenInfoDto) =>
            DbErrorHandler.GuardAsync(nameof(UpdateNetworkToken), async () =>
            {
                NetworkTokenInfo networkTokenInfo = NetworkTokenMapper.Map(networkTokenInfoDto)!;
                using CybsDbContext db = new();
                return await db.NetworkTokenInfos
                    .Where(model => model.PaymentTokenId == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.PaymentCardId, networkTokenInfo.PaymentCardId)
                        .SetProperty(m => m.TokenValue, networkTokenInfo.TokenValue)
                        .SetProperty(m => m.OriginalAccountExpMonth, networkTokenInfo.OriginalAccountExpMonth)
                        .SetProperty(m => m.OriginalAccountExpYear, networkTokenInfo.OriginalAccountExpYear)
                        .SetProperty(m => m.OriginalAccountNumber, networkTokenInfo.OriginalAccountNumber)
                        .SetProperty(m => m.OriginalAccountSuffix, networkTokenInfo.OriginalAccountSuffix)
                        .SetProperty(m => m.TokenizedCardType, networkTokenInfo.TokenizedCardType)
                        .SetProperty(m => m.PaymentAccountReferenceNumber, networkTokenInfo.PaymentAccountReferenceNumber)
                        .SetProperty(m => m.TokenAccountNumber, networkTokenInfo.TokenAccountNumber)
                        .SetProperty(m => m.TokenExpMonth, networkTokenInfo.TokenExpMonth)
                        .SetProperty(m => m.TokenExpYear, networkTokenInfo.TokenExpYear)
                        .SetProperty(m => m.TokenRequestorId, networkTokenInfo.TokenRequestorId)
                        .SetProperty(m => m.TokenState, networkTokenInfo.TokenState)
                        .SetProperty(m => m.EnrollmentId, networkTokenInfo.EnrollmentId)
                        .SetProperty(m => m.MitpreviousTransactionId, networkTokenInfo.MitpreviousTransactionId)
                        .SetProperty(m => m.ResponseTransactionJson, networkTokenInfo.ResponseTransactionJson)
                        );
            });

    }



}
