using System;

namespace UnitTestIssue.Models {
  public class ReminderOverview {
    public string InvestorId { get; set; }
    public string Investor { get; set; }
    public DateTime Due { get; set; }
    public bool Overdue { get; set; }
    public string Text { get; set; }
  }
}