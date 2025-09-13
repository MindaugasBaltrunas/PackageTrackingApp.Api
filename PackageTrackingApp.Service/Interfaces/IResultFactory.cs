using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.Interfaces
{
    public interface IResultFactory
    {
        Result<T> CreateSuccess<T>(T data);
        Result<T> CreateFailure<T>(string error);
        Result<T> CreateFailure<T>(List<string> errors);
    }
}
