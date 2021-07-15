namespace QckMox.Demo.Simple.Models
{
    public class ApiResponse<T>
    {

        public T Data { get; set; }
        public string Message { get; set; }
    }
}