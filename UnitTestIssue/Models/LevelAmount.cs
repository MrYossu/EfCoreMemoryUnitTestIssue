namespace UnitTestIssue.Models {
  public class LevelAmount {
    public int Id { get; set; }
    public int LevelId { get; set; }
    public Level Level { get; set; }
    public int Amount { get; set; }
    public int NumberOfShares { get; set; }
    public string GoCardlessLink { get; set; }
  }
}