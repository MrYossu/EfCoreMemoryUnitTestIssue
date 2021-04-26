using System;
using System.Collections.Generic;

namespace UnitTestIssue.Models {
  public class MaxSharesAtDate {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int MaxShares { get; set; }
    public List<Term> Terms { get; set; }
  }
}