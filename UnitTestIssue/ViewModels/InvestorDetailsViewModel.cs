using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Pixata.Extensions;
using UnitTestIssue.Models;

namespace UnitTestIssue.ViewModels {
  public class InvestorDetailsViewModel {
    private readonly AppDbContext _appDbContext;
    private DateTime _date;
    public Option<Investor>? Investor { get; private set; }
    private string _id;
    private Investor _investor;

    #region Ctor and SetInvestor

    public InvestorDetailsViewModel(AppDbContext appDbContext) =>
      _appDbContext = appDbContext;

    public async Task SetInvestor(string id, DateTime? date = null) {
      _date = date ?? DateTime.Today;
      Investor = await _appDbContext.Investors
        .Include(i => i.Payments)
        .ThenInclude(ip => ip.PaymentSource)
        .Include(i => i.Terms)
        .ThenInclude(t => t.LevelAmount)
        .ThenInclude(la => la.Level)
        .Include(i => i.Terms)
        .ThenInclude(t => t.Shares)
        .ThenInclude(s => s.Avreich)
        .Include(i => i.Terms)
        .ThenInclude(t => t.MaxSharesAtDate)
        .FirstOrDefaultAsync(i => i.Id == id);
      _id = id;
      Investor.Match(investor => {
          investor.IfSome(inv => {
            _investor = inv;
            Expired = inv.Terms.Any() && inv.Terms.OrderByDescending(t => t.Start).First().End < _date;
            if (inv.Terms.Any()) {
              SelectedTerm = inv.Terms.OrderByDescending(t => t.Start).First();
              LatestTermId = inv.Terms.OrderByDescending(t => t.Start).First().Id;
            }
          });
        },
        () => {
        });
    }

    #endregion

    #region Terms and renew

    public Term SelectedTerm { get; set; }
    public int LatestTermId { get; set; }
    public bool Expired { get; set; }

    public async Task<RenewalViewModel> NewRenewalViewModel(bool modify = false) {
      if (Investor == null) {
        // Can't actually happen, but we get a compiler warning without this, so let's keep VS happy eh?
        return new RenewalViewModel();
      }
      List<LevelAmount> levelAmounts = await _appDbContext.LevelAmounts.Include(la => la.Level).OrderBy(la => la.Amount).ToListAsync();
      return await Investor?.ToAsync().MapAsync(
        async inv => {
          Term latestTerm = inv.Terms.OrderByDescending(ila => ila.End).First();
          int selectedLevelAmountId = latestTerm.LevelAmountId;
          List<int> avreichIds = latestTerm.Shares.Select(s => s.AvreichId).ToList();
          ObservableCollection<Share> otherShares = (await _appDbContext.Shares
              .Where(s => avreichIds.Contains(s.AvreichId) && s.Term.Start <= _date && s.Term.End >= _date && s.Term.InvestorId != latestTerm.InvestorId)
              .ToListAsync())
            .Select(s => new Share {
              Id = s.Id,
              Quantity = s.Quantity,
              AvreichId = s.AvreichId,
              Avreich = s.Avreich
            })
            .ToObservableCollection();
          DateTime startOfNewTerm = modify
            ? latestTerm.Start
            : latestTerm.End.Date < _date
              ? _date.StartOfMonth()
              : latestTerm.End.AddDays(1).Date;
          DateTime endOfNewTerm = modify
            ? latestTerm.End.EndOfDay() // Can only modify if the last term hasn't run out, so we're safe in reusing the end of the current term
            : startOfNewTerm.AddYears(1).AddDays(-1).EndOfDay();
          return new RenewalViewModel {
            Modifying = modify,
            CurrentTermId = latestTerm.Id,
            MinStart = inv.Terms.Count == 1 ? DateTime.MinValue : inv.Terms.OrderByDescending(ila => ila.End).Skip(1).First().End.AddDays(1).Date,
            OriginalStart = startOfNewTerm,
            Start = startOfNewTerm,
            OriginalEnd = endOfNewTerm,
            End = endOfNewTerm,
            LevelAmounts = levelAmounts,
            LevelAmountsOvs = levelAmounts.OrderBy(la => la.LevelId).ThenBy(la => la.Amount).Select(la => (la.Id, $"{la.Level.Name} - £{la.Amount}/month ({la.NumberOfShares} shares)")).ToObservableCollection(),
            SelectedLevelAmountId = selectedLevelAmountId,
            Frequency = latestTerm.Frequency,
            DefaultPaymentMethod = latestTerm.DefaultPaymentMethod,
            DefaultPaymentSourceId = latestTerm.DefaultPaymentSourceId,
            MaxSharesPerAvreich = latestTerm.MaxSharesAtDate.MaxShares,
            Shares = latestTerm.Shares.Select(s => new Share {
              Id = s.Id,
              Quantity = s.Quantity,
              AvreichId = s.AvreichId,
              Avreich = s.Avreich
            }).ToObservableCollection(),
            OtherShares = otherShares,
            AvailableAvreichim = (await _appDbContext.Avreichim.Select(a => new { a.Id, a.FirstName, a.Surname, a.HebrewName, NShares = latestTerm.MaxSharesAtDate.MaxShares - a.Shares.Where(a => a.Term.Start <= _date && a.Term.End >= _date.EndOfDay()).Sum(s => s.Quantity) }).Where(a => a.NShares > 0).ToListAsync())
              .Select(a => new AvreichOverview { Id = a.Id, FirstName = a.FirstName, Surname = a.Surname, HebrewName = a.HebrewName, Shares = a.NShares })
              .ToObservableCollection()
          };
        }).IfNone(new RenewalViewModel());
    }

    public async Task<Either<string, Term>> OnSubmitRenew(RenewalViewModel rvm) =>
      await RenewTerm(rvm);

    private async Task<Either<string, Term>> RenewTerm(RenewalViewModel rvm) {
      Debug.WriteLine($"VM.RenewTerm - Shares in incoming VM: {rvm.Shares.Count}");
      int maxSharesAtDateId = (await _appDbContext.MaxSharesAtDates.OrderByDescending(m => m.Date).FirstAsync()).Id;
      Term term = CreateNewTerm(rvm, true, maxSharesAtDateId);
      Debug.WriteLine($"VM.RenewTerm - Shares in new VM before saving: {term.Shares.Count}");
      Either<string, Term> newTerm = await SaveTerm(term, rvm.Shares.ToList());
      Debug.WriteLine($"VM.RenewTerm - Shares in new VM after saving: {term.Shares.Count}");
      return newTerm;
    }

    private Term CreateNewTerm(RenewalViewModel rvm, bool external, int maxSharesAtDateId) =>
      new() {
        Start = rvm.Start,
        End = rvm.End,
        External = external,
        LevelAmountId = rvm.SelectedLevelAmountId,
        Frequency = rvm.Frequency,
        DefaultPaymentMethod = rvm.DefaultPaymentMethod,
        DefaultPaymentSourceId = rvm.DefaultPaymentSourceId,
        InvestorId = _id,
        MaxSharesAtDateId = maxSharesAtDateId,
        Description = rvm.Modifying ? "Adjusted" : "Renewal",
        Shares = rvm.Shares
          .Where(s => s.Quantity > 0)
          .Select(s => new Share {
            AvreichId = s.AvreichId,
            Quantity = s.Quantity
          })
          .ToList()
      };

    private async Task<Either<string, Term>> SaveTerm(Term term, List<Share> shares) {
      try {
        if (term.Id == 0) {
          await _appDbContext.Terms.AddAsync(term);
        } else {
          _appDbContext.Terms.Update(term);
        }
        await _appDbContext.SaveChangesAsync();
        SelectedTerm = term;
        LatestTermId = term.Id;
      }
      catch (Exception ex) {
        return ex.Messages();
      }
      shares.ForEach(s => {
        Share share = term.Shares.FirstOrDefault(s1 => s1.AvreichId == s.AvreichId);
        if (share != null) {
          share.Avreich = shares.Single(s1 => s1.AvreichId == s.AvreichId).Avreich;
        }
      });
      await SetBalanceBroughtForward();
      return term;
    }

    private async Task SetBalanceBroughtForward() {
      Term latestTerm = _investor.Terms.OrderByDescending(t => t.Start).First();
      // In the real code, we update latestTerm.BalanceBroughtForward. I omitted this for clarity
      Debug.WriteLine($"VM.SetBalanceBroughtForward - Shares in investor before saving: {latestTerm.Shares.Count}");
      _appDbContext.Terms.Update(latestTerm);
      await _appDbContext.SaveChangesAsync();
      Debug.WriteLine($"VM.SetBalanceBroughtForward - Shares in investor after saving: {latestTerm.Shares.Count}");
    }

    #endregion
  }
}