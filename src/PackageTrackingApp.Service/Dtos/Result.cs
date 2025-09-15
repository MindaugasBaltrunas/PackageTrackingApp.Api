
namespace PackageTrackingApp.Service.Dtos
{
    public class Result<T>
    {
        public bool IsSuccessful { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? ErrorMessage => Errors.FirstOrDefault();

        public Result() { }

        public Result(T data)
        {
            IsSuccessful = true;
            Data = data;
            Errors = new List<string>();
        }

        public Result(string error)
        {
            IsSuccessful = false;
            Data = default(T);
            Errors = new List<string> { error };
        }

        public Result(List<string> errors)
        {
            IsSuccessful = false;
            Data = default(T);
            Errors = errors;
        }
    }

}
