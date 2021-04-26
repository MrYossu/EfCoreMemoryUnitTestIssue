namespace UnitTestIssue.Models {
  public class AvreichOverview {
    public int Id { get; set; }
    public bool Active { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string FullName =>
      FirstName + " " + Surname;
    public string HebrewName { get; set; }
    public string SyndicateName { get; set; }
    public int Shares { get; set; }
  }
}