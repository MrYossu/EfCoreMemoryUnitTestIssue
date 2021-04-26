using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestIssue.Models {
  public class InvestorPayment {
    public InvestorPayment() =>
      Date = DateTime.Today;

    public int Id { get; set; }
    public string InvestorID { get; set; }
    public Investor Investor { get; set; }
    public DateTime Date { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    public PaymentMethods PaymentMethod { get; set; }
    public int PaymentSourceId { get; set; } = -1;
    public PaymentSource PaymentSource { get; set; }
    public string Notes { get; set; }
  }
}