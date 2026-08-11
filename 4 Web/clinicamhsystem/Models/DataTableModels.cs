using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Models
{
    public class DataTableRequest
    {
        [FromQuery(Name = "draw")]
        public int Draw { get; set; }

        [FromQuery(Name = "start")]
        public int Start { get; set; }

        [FromQuery(Name = "length")]
        public int Length { get; set; }

        [FromQuery(Name = "search[value]")]
        public string SearchValue { get; set; }

        [FromQuery(Name = "order[0][column]")]
        public int SortColumn { get; set; }

        [FromQuery(Name = "order[0][dir]")]
        public string SortDir { get; set; }
    }

    public class DataTableResponse<T>
    {
        public int Draw { get; set; }

        public int RecordsTotal { get; set; }

        public int RecordsFiltered { get; set; }

        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}
