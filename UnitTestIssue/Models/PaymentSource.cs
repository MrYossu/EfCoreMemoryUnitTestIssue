using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnitTestIssue.Models {
  public class PaymentSource {
    public const int DefaultId = -1;
    public PaymentSource() =>
      InvestorPayments = new();

    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public List<InvestorPayment> InvestorPayments { get; set; }

    public override string ToString() =>
      Name;
  }
}