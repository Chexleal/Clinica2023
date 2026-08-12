using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Models
{
    public class DataTableRequest
    {
        [FromForm(Name = "draw")]
        public int Draw { get; set; }

        [FromForm(Name = "start")]
        public int Start { get; set; }

        [FromForm(Name = "length")]
        public int Length { get; set; }

        [FromForm(Name = "search[value]")]
        public string SearchValue { get; set; }

        [FromForm(Name = "order[0][column]")]
        public int SortColumn { get; set; }

        [FromForm(Name = "order[0][dir]")]
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
