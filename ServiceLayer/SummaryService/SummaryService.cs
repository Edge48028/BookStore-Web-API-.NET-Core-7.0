﻿using DataLayer.EF;
using Microsoft.EntityFrameworkCore;
using ModelAndRequest.API;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.SummaryService
{
    public class SummaryService : ISummaryService
    {
        private readonly EShopDbContext eShopDbContext;

        public SummaryService(EShopDbContext eShopDbContext)
        {
            this.eShopDbContext = eShopDbContext;
        }

        public async Task<ApiResult<object>> Get()
        {
            var userCount = await eShopDbContext.Users.Where(x => x.isUser == true).CountAsync();
            var orderCount = await eShopDbContext.Orders.Where(x=>x.isDelete == false).CountAsync();
            var newOrderCount = await eShopDbContext.Orders.Where(x => x.OrderStatus == DataLayer.Enums.OrderStatus.DA_DAT_HANG && x.isDelete == false).CountAsync();
            var productCount = await eShopDbContext.Books.CountAsync();

            return new ApiResult<object>(success: true, messge: "Thanh cong", new { user = userCount, order = orderCount, newOrder = newOrderCount, product = productCount });
        }
    }
}