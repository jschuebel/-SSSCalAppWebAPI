namespace SSSCalAppWebAPI.Models
{
    public class FilterDTO<T>
    {
        public int total {get; set;}
        public T data {get; set;}
    }
}