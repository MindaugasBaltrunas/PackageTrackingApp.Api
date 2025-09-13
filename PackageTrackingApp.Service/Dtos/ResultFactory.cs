using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Service.Dtos
{
    public class ResultFactory : IResultFactory
    {
        public Result<T> CreateSuccess<T>(T data)
        {
            return new Result<T>(data);
        }

        public Result<T> CreateFailure<T>(string error)
        {
            return new Result<T>(error);
        }

        public Result<T> CreateFailure<T>(List<string> errors)
        {
            return new Result<T>(errors);
        }
    }
}
