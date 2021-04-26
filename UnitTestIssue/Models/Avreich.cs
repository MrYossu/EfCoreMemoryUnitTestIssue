using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestIssue.Models {
  public class Avreich {
    public Avreich() {
      Shares = new List<Share>();
      Payments = new List<AvreichPayment>();
    }

    public bool Active { get; set; } = true;
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string Surname { get; set; }

    [NotMapped]
    public string FullName =>
      FirstName + " " + Surname;

    [Display(Name = "Hebrew name")]
    public string HebrewName { get; set; }

    public string SyndicateName { get; set; }

    public List<Share> Shares { get; set; }

    public List<AvreichPayment> Payments { get; set; }
  }
}