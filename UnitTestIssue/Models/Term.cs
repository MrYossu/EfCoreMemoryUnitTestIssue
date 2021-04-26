using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestIssue.Models {
  public class Term {
    public Term() =>
      Shares = new();

    public int Id { get; set; }
    public string InvestorId { get; set; }
    public Investor Investor { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool External { get; set; }
    public string Description { get; set; }
    public int LevelAmountId { get; set; }
    public LevelAmount LevelAmount { get; set; }
    public PaymentMethods DefaultPaymentMethod { get; set; }
    public int DefaultPaymentSourceId { get; set; }
    public PaymentSource DefaultPaymentSource { get; set; }
    public Frequency Frequency { get; set; }
    public int MaxSharesAtDateId { get; set; }
    public MaxSharesAtDate MaxSharesAtDate { get; set; }
    public List<Share> Shares { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    // NOTE - Despite the slightly misleading name, the BBF is the amount over/underpaid at the end of *this* term (including any BBF from previous terms). Perhaps a more accurate name would be BalanceSentForwardToTheNextTerm :)
    public decimal BalanceBroughtForward { get; set; }

    public override string ToString() =>
      $"Id: {Id}, InvestorID: {InvestorId}, Start: {Start.ToShortDateString()}, End: {End.ToShortDateString()}, LevelAmountId: {LevelAmountId}, Number of shares: {Shares.Count}, BBF: {BalanceBroughtForward}";
  }
}