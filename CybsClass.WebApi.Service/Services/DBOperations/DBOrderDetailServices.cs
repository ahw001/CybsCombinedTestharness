using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBOrderDetailServices
    {
        public static Task<DbResult<int>> GetPaymentCardCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetPaymentCardCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.OrderDetails.CountAsync();
            });
        public static async Task<List<OrderDetailDto>> GetOrderDetails()
        {
            try
            {
                Console.WriteLine("Geting full list of Order Details ...");
                using CybsDbContext db = new();
                var orderDetails = await db.OrderDetails.ToListAsync();
                List<OrderDetailDto> orderDetailDtos = OrderDetailMapper.Map(orderDetails)!;
                return orderDetailDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetOrderDetails), ex);
                var orderDetails = new List<OrderDetailDto>();
                OrderDetailDto orderDetailDto = new OrderDetailDto();
                orderDetailDto.Error = DbErrorHandler.BuildError(ex, nameof(GetOrderDetails));
                orderDetails.Add(orderDetailDto);
                return orderDetails;
            }

        }

        public static async Task<OrderDetailDto?> GetOrderDetailByUsingId(int orderdetailid)
        {
            try
            {
                Console.WriteLine($"Geting Order Detail for: {orderdetailid}");
                using CybsDbContext db = new();
                Task<OrderDetail?> task = db.OrderDetails.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.OrderId == orderdetailid);

                var orderDetail = await task;
                if (orderDetail == null)
                {
                    return null;
                }
                return OrderDetailMapper.Map(orderDetail);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetOrderDetailByUsingId), ex);
                var orderDetailDto = new OrderDetailDto();
                orderDetailDto.Error = DbErrorHandler.BuildError(ex, nameof(GetOrderDetailByUsingId));
                return orderDetailDto;
            }
        }
        public static Task<DbResult<int>> CreateOrderDetail(OrderDetailDto orderDetailDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateOrderDetail), async () =>
            {
                OrderDetail orderDetail = OrderDetailMapper.Map(orderDetailDto)!;
                using CybsDbContext db = new();
                db.OrderDetails.Add(orderDetail);
                return await db.SaveChangesAsync();
            });
    }
}
