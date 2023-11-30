using ModelAndRequest.API;
using ModelAndRequest.Category;
using System.Threading.Tasks;

namespace ServiceLayer.CategoryServices
{
    public interface ICategoryService
    {
        Task<ApiResult<bool>> AddCategory(CategoryRequest categoryRequest);

        Task<ApiResult<bool>> EditCategory(int id, CategoryRequest categoryRequest);

        Task<ApiResult<object>> GetAllCategory();

        Task<ApiResult<object>> GetAllBook(int id);
    }
}