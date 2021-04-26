using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestIssue.Models {
  public class AvreichPayment {
    public int Id { get; set; }
    public int AvreichID { get; set; }
    public Avreich Avreich { get; set; }
    public DateTime Date { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
  }
}