using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Pixata.Extensions;
using UnitTestIssue.Models;
using UnitTestIssue.ViewModels;

namespace UnitTestIssue.Tests {
  [TestClass]
  public class InvestorDetailsViewModelTests {
    private AppDbContext _appDbContext;

    #region Data

    private string _investorExpiredOneTermId;
    private string _investorWithHalfSharesOneTermId;
    private string _investorWithFullSharesOneTermId;
    private string _investorWithOneAndAHalfSharesOneTermId;
    private string _investorExpiredTwoTermsId;
    private string _investorWithHalfSharesTwoTermsId;
    private string _investorWithFullSharesTwoTermsId;
    private string _investorWithOneAndAHalfSharesTwoTermsId;
    private string _investorWithThreeTermsId;

    private readonly int _investorWithHalfSharesOneTerm_TermId = 10;

    private static readonly DateTime _date2018Start = new(2018, 1, 1);
    private static readonly DateTime _date2018End = new DateTime(2018, 12, 31).EndOfDay();
    private static readonly DateTime _date2019Start = new(2019, 1, 1);
    private static readonly DateTime _date2019End = new DateTime(2019, 12, 31).EndOfDay();
    private static readonly DateTime _date2020Start = new(2020, 1, 1);
    private static readonly DateTime _date2020End = new DateTime(2020, 12, 31).EndOfDay();
    private static readonly DateTime _date2021Start = new(2021, 1, 1);
    private static readonly DateTime _date2021End = new DateTime(2021, 12, 31).EndOfDay();
    private static readonly DateTime _dateMyBirthday2019 = new(2019, 4, 7);
    private static readonly DateTime _dateMyBirthday2020 = new(2020, 4, 7);
    private static readonly DateTime _dateMyBirthday2021 = new(2021, 4, 7);

    private readonly Avreich _avreich1 = new() { Id = 1, FirstName = "Yankel", Surname = "Shmerel" };
    private readonly Avreich _avreich2 = new() { Id = 2, FirstName = "Berel", Surname = "Yankelovsky" };
    private readonly Avreich _avreich3 = new() { Id = 3, FirstName = "Yonah Yosef", Surname = "Yonikins" };

    #endregion

    #region Utility

    private InvestorDetailsViewModel GetInvestorDetailsViewModel() =>
      new(_appDbContext);

    #endregion

    #region Test initialise and cleanup

    [TestInitialize]
    public async Task InitialiseData() {
      _appDbContext = await TestData.GetAppDbContext();

      List<Level> levels = TestData.Levels;

      MaxSharesAtDate maxShares = new() { Id = 1, Date = _date2020Start };
      await _appDbContext.MaxSharesAtDates.AddAsync(maxShares);

      Investor investorExpiredOneTerm = new() {
        FirstName = "Jim",
        Surname = "investorExpiredOneTerm",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 15,
            Start = _date2019Start,
            End = _date2019End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Business300,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Business300),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 115, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorExpiredOneTerm);
      _investorExpiredOneTermId = investorExpiredOneTerm.Id;

      Investor investorHalfOneTerm = new() {
        FirstName = "Jim",
        Surname = "investorHalfOneTerm",
        Email = "jim",
        Terms = new() {
          new() {
            Id = _investorWithHalfSharesOneTerm_TermId,
            Start = _date2020Start,
            End = _date2020End,
            External = true,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Business300,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Business300),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 100, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorHalfOneTerm);
      _investorWithHalfSharesOneTermId = investorHalfOneTerm.Id;

      Investor investorFullOneTerm = new() {
        FirstName = "Jim",
        Surname = "investorFullOneTerm",
        Email = "jammy",
        Terms = new() {
          new() {
            Id = 20,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Corporate600,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Corporate600),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 110, Quantity = 12, AvreichId = _avreich2.Id, Avreich = _avreich2 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorFullOneTerm);
      _investorWithFullSharesOneTermId = investorFullOneTerm.Id;

      Investor investorOneAndAHalfOneTerm = new() {
        FirstName = "Jim",
        Surname = "investorOneAndAHalfOneTerm",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 30,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Premiere900,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Premiere900),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 120, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 },
              new() { Id = 121, Quantity = 12, AvreichId = _avreich3.Id, Avreich = _avreich3 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorOneAndAHalfOneTerm);
      _investorWithOneAndAHalfSharesOneTermId = investorOneAndAHalfOneTerm.Id;

      Investor investorExpiredTwoTerms = new() {
        FirstName = "Jim",
        Surname = "investorExpiredTwoTerms",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 115,
            Start = new(2018, 1, 1),
            End = new DateTime(2018, 12, 31).EndOfDay(),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 215,
            Start = _date2019Start,
            End = _date2019End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Business300,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Business300),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 1115, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorExpiredTwoTerms);
      _investorExpiredTwoTermsId = investorExpiredTwoTerms.Id;

      Investor investorHalfTwoTerms = new() {
        FirstName = "Jim",
        Surname = "investorHalfTwoTerms",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 110,
            Start = _date2019Start,
            End = _date2019End,
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 210,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Business300,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Business300),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 1100, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorHalfTwoTerms);
      _investorWithHalfSharesTwoTermsId = investorHalfTwoTerms.Id;

      Investor investorFullTwoTerms = new() {
        FirstName = "Jim",
        Surname = "investorFullTwoTerms",
        Email = "jammy",
        Terms = new() {
          new() {
            Id = 120,
            Start = _date2019Start,
            End = _date2019End,
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 220,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Corporate600,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Corporate600),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 1110, Quantity = 12, AvreichId = _avreich2.Id, Avreich = _avreich2 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorFullTwoTerms);
      _investorWithFullSharesTwoTermsId = investorFullTwoTerms.Id;

      Investor investorOneAndAHalfTwoTerms = new() {
        FirstName = "Jim",
        Surname = "investorOneAndAHalfTwoTerms",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 130,
            Start = _date2019Start,
            End = _date2019End,
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 230,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Premiere900,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Premiere900),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
            Shares = new() {
              new() { Id = 1120, Quantity = 6, AvreichId = _avreich1.Id, Avreich = _avreich1 },
              new() { Id = 2121, Quantity = 12, AvreichId = _avreich3.Id, Avreich = _avreich3 }
            }
          }
        }
      };
      await _appDbContext.Investors.AddAsync(investorOneAndAHalfTwoTerms);
      _investorWithOneAndAHalfSharesTwoTermsId = investorOneAndAHalfTwoTerms.Id;

      Investor investorWithThreeTerms = new() {
        FirstName = "Jim",
        Surname = "investorWithThreeTerms",
        Email = "jim",
        Terms = new() {
          new() {
            Id = 140,
            Start = _date2018Start,
            End = _date2018End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Syndicate100,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Syndicate100),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 141,
            Start = _date2019Start,
            End = _date2019End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Syndicate100,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Syndicate100),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
          new() {
            Id = 142,
            Start = _date2020Start,
            End = _date2020End,
            MaxSharesAtDateId = 1,
            LevelAmountId = TestData.LevelAmount_Syndicate100,
            LevelAmount = levels.SelectMany(l => l.Amounts).Single(la => la.Id == TestData.LevelAmount_Syndicate100),
            DefaultPaymentMethod = PaymentMethods.StandingOrder,
            DefaultPaymentSourceId = TestData.PaymentSource_StandingOrder,
          },
        }
      };
      await _appDbContext.Investors.AddAsync(investorWithThreeTerms);
      _investorWithThreeTermsId = investorWithThreeTerms.Id;

      await _appDbContext.SaveChangesAsync();
    }

    [TestCleanup]
    public void TestCleanup() =>
      _appDbContext.Dispose();

    #endregion

    [TestMethod]
    public async Task InvestorDetailsViewModel_OnSubmitRenew_Renew_OnlyInvestorInThisAvreich_AddedThreeSharesInAnotherSameAvreich() {
      InvestorDetailsViewModel investorDetailsVm = GetInvestorDetailsViewModel();
      await investorDetailsVm.SetInvestor(_investorWithHalfSharesOneTermId, _dateMyBirthday2020);
      RenewalViewModel newRenewalVm = await investorDetailsVm.NewRenewalViewModel();
      Debug.WriteLine($"Test -newRenewalVm.Shares.Count: {newRenewalVm.Shares.Count}");
      Share addedShare = new() { AvreichId = _avreich2.Id, Quantity = 7 };
      newRenewalVm.Shares.Add(addedShare);
      Debug.WriteLine($"Test -newRenewalVm.Shares.Count after adding extra: {newRenewalVm.Shares.Count}");
      await investorDetailsVm.OnSubmitRenew(newRenewalVm);
      Investor modifiedInvestor = await _appDbContext.Investors.SingleAsync(i => i.Id.ToString() == _investorWithHalfSharesOneTermId);
      Term latestTerm = modifiedInvestor.Terms.OrderByDescending(t => t.Start).First();
      Debug.WriteLine($"Test -latestTerm.Shares.Count: {latestTerm.Shares.Count}");
      Assert.AreEqual(2, latestTerm.Shares.Count);
    }

  }
}