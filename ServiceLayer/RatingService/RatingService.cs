﻿using DataLayer.EF;
using DataLayer.Entities;
using ModelAndRequest.API;
using ModelAndRequest.Rating;
using System.Linq;

namespace ServiceLayer.RatingService
{
    /// <summary>
    /// MAX, DONT_BUY, NO_BOOK, SUCCESS
    /// </summary>
    public class RatingService : IRatingService
    {
        private readonly EShopDbContext eShopDbContext;

        public RatingService(EShopDbContext eShopDbContext)
        {
            this.eShopDbContext = eShopDbContext;
        }

        public ApiResult<string> AddRating(RatingRequest ratingRequest)
        {
            var countVote = eShopDbContext.Users.Find(ratingRequest.UserId)?.BookRatings?.Where(x => x.BookId == ratingRequest.BookId)?.Count();
            if (countVote > 4)
                return new ApiResult<string>(success: false, messge: "Bạn đã đánh giá trước đó nhiều lần !", payload: "MAXIMUM");

            var book = eShopDbContext.Books.Find(ratingRequest.BookId);
            if (book == null)
                return new ApiResult<string>(success: false, messge: "Không tìm thấy cuốn sách mà bạn muốn đánh giá", payload: "NO_BOOK");

            /// kiem tra kkhac hang da mua sp hay chua
            var isBuy = false;
            eShopDbContext.Users.Find(ratingRequest.UserId).Orders.ForEach(order =>
            {
                order.OrderDetails.ForEach(orderDetail =>
                {
                    if (orderDetail.BookId == ratingRequest.BookId)
                    {
                        isBuy = true;
                        return;
                    }
                    if (isBuy) return;
                });
            });

            if (!isBuy)
                return new ApiResult<string>(success: false, messge: "Chưa mua sao đánh giá được 😀", payload: "DONT_BUY");

            var rating = new BookRating()
            {
                BookId = ratingRequest.BookId,
                Comment = ratingRequest.Comment,
                UserId = ratingRequest.UserId,
                Rating = ratingRequest.Rating,
            };

            //diem tru di do vote se gap doi neu vote 1 sao va bag neu vote 2* , khong tru neu vote 3 sao (Tam on)
            book.WeekScore += (3 - ratingRequest.Rating) * 10;
            book.MonthScore += (3 - ratingRequest.Rating) * 10;
            book.YearScore += (3 - ratingRequest.Rating) * 10;

            eShopDbContext.BookRatings.Add(rating);
            eShopDbContext.SaveChangesAsync();
            return new ApiResult<string>(success: true, messge: "Đã gửi đánh giá của bạn ✔", payload: "SUCCESS");
        }
    }
}