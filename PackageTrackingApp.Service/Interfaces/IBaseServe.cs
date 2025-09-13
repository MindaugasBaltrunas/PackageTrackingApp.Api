using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        Task<Result<T>> AddEntityAsync(T entity);
    }
}

