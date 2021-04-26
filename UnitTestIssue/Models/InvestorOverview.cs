using System;

namespace UnitTestIssue.Models {
  public class InvestorOverview {
    public string Id { get; set; }
    public bool Active { get; set; }
    public string Name { get; set; }
    public string Level { get; set; }
    public int Amount { get; set; }
    public DateTime Renewal { get; set; }
    public PaymentMethods PaymentMethod { get; set; }
    public PaymentSource PaymentSource { get; set; }
  }
}