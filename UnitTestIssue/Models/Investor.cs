using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Pixata.Extensions;

namespace UnitTestIssue.Models {
  public class Investor : IdentityUser {
    public Investor() {
      Payments = new();
      Terms = new();
    }

    #region Properties

    public bool Active { get; set; } = true;

    [Display(Name = "first name")]
    public string FirstName { get; set; }

    [Display(Name = "surname")]
    public string Surname { get; set; }

    [NotMapped]
    public string FullName =>
      $"{FirstName} {Surname}";

    [Display(Name = "Hebrew name")]
    public string HebrewName { get; set; }

    public string Address { get; set; }

    public string Mobile { get; set; }

    public string Kollel { get; set; }

    public bool DirectDebitSetUp { get; set; }

    public DateTime? ReminderDate { get; set; }

    public string ReminderText { get; set; }

    public List<InvestorPayment> Payments { get; set; }

    public List<Term> Terms { get; set; }

    #endregion

    #region Methods

    public DateTime Renewal(DateTime date) =>
      TermOnDate(date).Match(sd => sd.End, Terms.Any() ? Terms.OrderByDescending(t => t.Start).First().End : DateTime.MinValue);

    public bool Premium(DateTime date) =>
      NumberOfAvreichim(date) > 1;

    public int InvestmentPrice(DateTime date) =>
      TermOnDate(date, true).Match(term => {
        DateTime d = term.Start.EndOfMonth();
        int investment = 0;
        while (d <= term.End.EndOfDay()) {
          investment += TermOnDate(d).Match(t => t.LevelAmount.Amount, 0);
          d = d.AddDays(1).EndOfMonth();
        }
        return investment;
      }, 0);

    public string InvestmentPriceNote(DateTime date) =>
      History(date).Match(terms => terms.Count == 1 ? "" : "<br/>" + string.Join(", ", terms.Select(t => $"&pound;{t.LevelAmount.Amount}/month from {t.Start.ToString("MMM yyyy")} to {t.End.ToString("MMM yyyy")}")),
        "");

    public int ExpectedPaymentForPeriod(DateTime date) =>
      TermOnDate(date).Match(term => {
        return term.Frequency switch {
          Frequency.Weekly => term.LevelAmount.Amount, // Assume 4 weeks in a month, and use 1 month's investment as his expected amount
          Frequency.Monthly => term.LevelAmount.Amount,
          Frequency.Quarterly => 3 * term.LevelAmount.Amount,
          Frequency.Biannually => 6 * term.LevelAmount.Amount,
          Frequency.Annually => 12 * term.LevelAmount.Amount,
          _ => 0
        };
      }, 0);

    public (DateTime Start, DateTime End) PreviousPeriod(DateTime date) =>
      TermOnDate(date).Match(term => {
          DateTime startOfPreviousMonth = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
          return term.Frequency switch {
            Frequency.Weekly => (startOfPreviousMonth, startOfPreviousMonth.EndOfMonth()),
            Frequency.Monthly => (startOfPreviousMonth, startOfPreviousMonth.EndOfMonth()),
            Frequency.Quarterly => (startOfPreviousMonth.AddMonths(-2), startOfPreviousMonth.EndOfMonth()),
            Frequency.Biannually => (startOfPreviousMonth.AddMonths(-5), startOfPreviousMonth.EndOfMonth()),
            Frequency.Annually => (startOfPreviousMonth.AddMonths(-11), startOfPreviousMonth.EndOfMonth()),
            _ => (DateTime.MinValue, DateTime.MinValue)
          };
        },
        (DateTime.MinValue, DateTime.MinValue));

    public decimal ActualPaymentForPeriod(DateTime date) {
      (DateTime start, DateTime end) = PreviousPeriod(date);
      return Payments.Where(p => p.Date >= start && p.Date <= end).Sum(p => p.Amount);
    }

    public (string type, string name) SyndicateOrKollel(DateTime date) =>
      !SharesAtDate(date).Any()
        ? ("", "")
        : NumberOfAvreichim(date) == 1
          ? ("Syndicate", SharesAtDate(date).Single().Avreich.SyndicateName ?? "")
          : SharesAtDate(date).Count() == 2 && SumOfSharesAtDate(date) < 2 * MaxSharesAtDate(date)
            ? ("", "")
            : ("Kollel", Kollel ?? "");

    public string PaymentMessage(DateTime date) =>
      TermOnDate(date).Match(
        sd => sd.DefaultPaymentMethod == PaymentMethods.DirectDebit || sd.DefaultPaymentMethod == PaymentMethods.StandingOrder
          ? $"We receive your payment via {sd.DefaultPaymentMethod.ToString().SplitCamelCase().ToLower()}. Many thanks for choosing to pay this way."
          : $"We receive your payment in {sd.Frequency.ToString().ToLower()} installations, your next payment is due on {sd.End.ToPrettyString()}."
        , "");

    public string Partner(DateTime date) =>
      string.Join(", ", SharesAtDate(date).Select(s => PartnerName(s.Avreich, date, s.Quantity)));

    public string PartnerNameWithPercentage(DateTime date) =>
      string.Join(", ", SharesAtDate(date).Select(s => PartnerName(s.Avreich, date, s.Quantity, Premium(date))));

    private string PartnerName(Avreich avreich, DateTime date, int qty, bool addPercentage = false) {
      int maxSharesAtDate = MaxSharesAtDate(date);
      return qty < maxSharesAtDate / 2
        ? "R' " + avreich.Id + Percentage(addPercentage, qty, maxSharesAtDate)
        : "R' " + avreich.FullName + Percentage(addPercentage, qty, maxSharesAtDate);
    }

    private string Percentage(bool addPercentage, int qty, int max) =>
      addPercentage
        ? $" ({qty.ToPercentageString(max)})"
        : "";

    public int AmountPaidThisTerm(DateTime date) =>
      TermOnDate(date, true)
        .Match(term => {
          return (int)Payments.Where(p => p.Date >= term.Start && p.Date <= term.End).Sum(p => p.Amount);
        }, 0);

    public int TransferredToYourAch(DateTime date) =>
      // TODO AYS - This is wrong, as it is including all terms, not just the one current on the date specified
      (int)History(date)
        .Match(terms => {
            Term first = terms.OrderByDescending(t => t.Start).First();
            Term last = terms.OrderBy(t => t.Start).First();
            DateTime start = first.Start;
            DateTime end = last.End;
            return terms.Select(term => term.Shares.Select(s => (double)s.Avreich.Payments.Where(p => p.Date >= start && p.Date <= end).Sum(p => p.Amount) * s.Quantity / MaxSharesAtDate(date)).Sum()).Sum();
          },
        0);

    public bool OverlappingTerms =>
      Terms.Count >= 2 && Terms.OrderBy(t => t.Start).Zip(Terms.OrderBy(t => t.Start).Skip(1)).Any(zip => zip.Item2.Start <= zip.Item1.End);

    public Option<List<Term>> History(DateTime date) =>
      TermOnDate(date, true)
        .Match(term => Terms.OrderBy(t => t.Start).Where(t => t.Start >= term.Start && t.End <= term.End.EndOfDay()).ToList(), Option<List<Term>>.None);

    public decimal HeldOnAccount(DateTime date) =>
      Math.Max(BalanceBroughtForward(date) + AmountPaidThisTerm(date) - TransferredToYourAch(date), 0);

    public decimal Outstanding(DateTime date) =>
      Math.Max(InvestmentPrice(date) - AmountPaidThisTerm(date) - BalanceBroughtForward(date), 0);

    public decimal BalanceBroughtForward(DateTime date) =>
      TermOnDate(date)
        .Match(term => Terms.OrderByDescending(t => t.Start).SkipWhile(t => t.Start != term.Start).Skip(1).FirstOrDefault()?.BalanceBroughtForward ?? 0m, 0m);

    public string PartnershipLevelCaption(DateTime date) =>
      SumOfSharesAtDate(date) <= MaxSharesAtDate(date) ? "Partnership level" : "Number of members";

    public string PartnershipLevel(DateTime date) {
      int maxSharesAtDate = MaxSharesAtDate(date);
      int sumOfSharesAtDate = SumOfSharesAtDate(date);
      return sumOfSharesAtDate == 0
        ? "0%"
        : sumOfSharesAtDate switch {
          int n when n <= maxSharesAtDate => (100 * n / (double)maxSharesAtDate).ToString("F0") + "%",
          int n when n > maxSharesAtDate => ((double)n / maxSharesAtDate).ToString(),
        };
    }

    public Option<Term> TermOnDate(DateTime date, bool externalOnly = false) {
      if (!Terms.Any()) {
        return Option<Term>.None;
      }
      if (!externalOnly) {
        // TODO AYS - If we find more than one, an exception will be thrown, so we need to log it and return something sensible, such as the last term
        return Terms.SingleOrDefault(t => t.Start.Date <= date && t.End.EndOfDay() >= date);
      }
      // Look for the last external whose start date is before date. This can happen if the date is before he beginning of the first term
      Term first = Terms.OrderBy(t => t.Start).LastOrDefault(t => t.External && t.Start <= date);
      if (first == null) {
        return Option<Term>.None;
      }
      // Now look for an external term after "first"
      Term last = Terms.OrderBy(t => t.Start).FirstOrDefault(t => t.External && t.Start > first.Start);
      if (last != null) {
        // The previous term is our end point. If that previous one is the same one (compare IDs), just return that
        if (first.Id == last.Id) {
          return first;
        }
        // Our target last term is the term before "last"
        last = Terms.OrderBy(t => t.Start).Last(t => t.Id != last.Id);
      } else {
        // If we don't find an external term after "first", then the last term of all is our end point if and only if its end date is before "date". If its after, then the subscription has expired and we return None (subject to the comment lower down)
        last = Terms.OrderBy(t => t.Start).Last();
        if (date > last.End.EndOfDay()) {
          return Option<Term>.None;
        }
      }
      return new Term {
        Start = first.Start.Date,
        End = last.End.EndOfDay(),
        Shares = Terms.Where(t => t.Start >= first.Start && t.End <= last.End.EndOfDay()).SelectMany(t => t.Shares).ToList()
      };
    }

    /// <summary>
    /// Returns the amount that the investor should have paid in between the two dates. It assumes that it will be used for full terms, and that all terms start on the 1st of a month and finish on the last day of a month
    /// </summary>
    /// <param name="start">The beginning of the period (usually a term)</param>
    /// <param name="end">The end of the period</param>
    /// <returns>The total expected payment</returns>
    public int ExpectedPaymentsBetweenDates(DateTime start, DateTime end) =>
      Enumerable.Range(0, Math.Abs(12 * (start.Year - end.Year) + start.Month - end.Month) + 1)
        .Sum(n => TermOnDate(start.AddMonths(n)).Match(t => t.LevelAmount.Amount, () => 0));

    public int NumberOfAvreichim(DateTime date) =>
      SharesAtDate(date).Count();

    public IEnumerable<Share> SharesAtDate(DateTime date) =>
      TermOnDate(date).Match(t => t.Shares, new List<Share>());

    public int SumOfSharesAtDate(DateTime date) =>
      SharesAtDate(date).Sum(s => s.Quantity);

    public int MaxSharesAtDate(DateTime date) =>
      TermOnDate(date).Match(t => t.MaxSharesAtDate.MaxShares, 0);

    public List<(DateTime date, int amount)> ReceivedByYourAch(DateTime date) {
      DateTime month3 = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
      DateTime month2 = new(month3.AddMonths(-1).Year, month3.AddMonths(-1).Month, 1);
      DateTime month1 = new(month2.AddMonths(-1).Year, month2.AddMonths(-1).Month, 1);
      int maxSharesAtDate = MaxSharesAtDate(date);
      return maxSharesAtDate == 0
        ? new List<(DateTime date, int amount)> {
          (month1, 0),
          (month2, 0),
          (month3, 0),
        }
        : new List<(DateTime date, int amount)> {
          (month1, (int)SharesAtDate(month1).Sum(s => s.Avreich.Payments.Where(p => p.Date.Year == month1.Year && p.Date.Month == month1.Month).Select(p => p.Amount * s.Quantity / maxSharesAtDate).Sum())),
          (month2, (int)SharesAtDate(month2).Sum(s => s.Avreich.Payments.Where(p => p.Date.Year == month2.Year && p.Date.Month == month2.Month).Select(p => p.Amount * s.Quantity / maxSharesAtDate).Sum())),
          (month3, (int)SharesAtDate(month3).Sum(s => s.Avreich.Payments.Where(p => p.Date.Year == month3.Year && p.Date.Month == month3.Month).Select(p => p.Amount * s.Quantity / maxSharesAtDate).Sum())),
        };
    }

    public string Report(DateTime date, StatementInformation statementInfo) =>
      statementInfo.ReportText.Replace("@AVREICHLEARNS@", Premium(date) ? "Your Yungerleit learn" : $"{Partner(date)} learns");

    #endregion
  }
}