namespace ClinicaServices
{
    public class PagedResult<T>
    {
        public int Total { get; set; }

        public int TotalFiltered { get; set; }

        public List<T> Data { get; set; } = new();
    }
}
