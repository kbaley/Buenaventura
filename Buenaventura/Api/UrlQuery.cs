namespace Buenaventura.Api
{
  public class UrlQuery
  {
      public int Page { get; set; } = 0;
      public int PageSize { get; set; } = 50;
      public string? Search { get; set; }
  }

  public class ReportQuery
  {
      public int? Year { get; set; }

      public int SelectedYear => Year ?? DateTime.Today.Year;

      public DateTime EndDate {
          get {
              var daysInMonth = DateTime.DaysInMonth(SelectedYear, DateTime.Today.Month);
              return SelectedYear == DateTime.Today.Year ?
                new DateTime(SelectedYear, DateTime.Today.Month, daysInMonth) :
                new DateTime(SelectedYear, 12, 31);
          }
      }

      public DateTime StartDate => new(1, 1, SelectedYear);
  }
}
