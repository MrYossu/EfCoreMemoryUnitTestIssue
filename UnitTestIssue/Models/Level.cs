using System.Collections.Generic;

namespace UnitTestIssue.Models {
  public class Level {
    public Level() =>
      Amounts = new();

    public int Id { get; set; }
    public string Name { get; set; }
    public List<LevelAmount> Amounts { get; set; }
  }
}