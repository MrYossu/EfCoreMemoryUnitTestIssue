namespace UnitTestIssue.Models {
  public class Share {
    public int Id { get; set; }
    public int TermId { get; set; }
    public Term Term { get; set; }
    public int AvreichId { get; set; }
    public Avreich Avreich { get; set; }
    public int Quantity { get; set; }
  }
}