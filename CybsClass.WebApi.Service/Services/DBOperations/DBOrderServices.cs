using Microsoft.EntityFrameworkCore;
using CybsClass.EntityModels;
using CybsClass.Cybersource.Models.DTOs;
using CybsClass.Cybersource.Models.Mappers;

namespace CybsClass.WebApi.Service.Services.DBOperations
{
    public class DBOrdersServices
    {
        public static Task<DbResult<int>> GetOrdersCountAsync() =>
            DbErrorHandler.GuardAsync(nameof(GetOrdersCountAsync), async () =>
            {
                using CybsDbContext db = new();
                return await db.Orders.CountAsync();
            });
        public static async Task<List<OrderDto>> GetOrders()
        {
            try
            {
                Console.WriteLine("Geting full list of Orders ...");
                using CybsDbContext db = new();
                var order = await db.Orders.ToListAsync();
                List<OrderDto> orderDtos = OrderMapper.Map(order)!;
                return orderDtos;
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetOrders), ex);
                var order = new List<OrderDto>();
                OrderDto orderDto = new OrderDto();
                orderDto.Error = DbErrorHandler.BuildError(ex, nameof(GetOrders));
                order.Add(orderDto);
                return order;
            }

        }

        public static async Task<OrderDto?> GetOrdersByUsingId(int orderid)
        {
            try
            {
                Console.WriteLine($"Geting Order for: {orderid}");
                using CybsDbContext db = new();
                Task<Order?> task = db.Orders.AsNoTracking()
                            .FirstOrDefaultAsync(model => model.OrderId == orderid);

                var order = await task;
                if (order == null)
                {
                    return null;
                }
                return OrderMapper.Map(order);
            }
            catch (Exception ex)
            {
                DbErrorHandler.Log(nameof(GetOrdersByUsingId), ex);
                var orderDto = new OrderDto();
                orderDto.Error = DbErrorHandler.BuildError(ex, nameof(GetOrdersByUsingId));
                return orderDto;
            }
        }
        public static Task<DbResult<int>> CreateOrders(OrderDto orderDto) =>
            DbErrorHandler.GuardAsync(nameof(CreateOrders), async () =>
            {
                Order order = OrderMapper.Map(orderDto)!;
                using CybsDbContext db = new();
                db.Orders.Add(order);
                return await db.SaveChangesAsync();
            });
    }
}
