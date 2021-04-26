using System;
using System.Linq;

namespace UnitTestIssue.Models {
  public static class InvestorExtensions {
    public static InvestorOverview ToOverview(this Investor investor) {
      Term term = investor.Terms.OrderByDescending(ila => ila.End).FirstOrDefault();
      return new InvestorOverview {
        Id = investor.Id,
        Active = investor.Active,
        Name = investor.FullName,
        Level = term?.LevelAmount?.Level?.Name ?? "",
        Amount = term?.LevelAmount?.Amount ?? 0,
        Renewal = investor.Renewal(DateTime.Today),
        PaymentMethod = term?.DefaultPaymentMethod ?? PaymentMethods.Manual,
        PaymentSource = term?.DefaultPaymentSource
      };
    }
  }
}