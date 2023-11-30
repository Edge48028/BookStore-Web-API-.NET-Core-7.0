using ModelAndRequest.API;
using ModelAndRequest.Rating;

namespace ServiceLayer.RatingService
{
    public interface IRatingService
    {
        ApiResult<string> AddRating(RatingRequest ratingRequest);
    }
}