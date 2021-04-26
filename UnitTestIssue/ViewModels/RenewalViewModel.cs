using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Achim.Web.ViewModels;
using Pixata.Extensions;
using UnitTestIssue.Models;

namespace UnitTestIssue.ViewModels {
  public class RenewalViewModel {
    public RenewalViewModel() {
      LevelAmounts = new();
      LevelAmountsOvs = new();
      LevelsOvs = new();
      Shares = new();
      OtherShares = new();
      AvailableAvreichim = new();
    }

    public bool Modifying { get; set; }

    public string ActionText =>
      Modifying
        ? AdjustOrCorrect switch {
          1 => "Correct",
          2 => "Adjust",
          _ => "Choose \"correcting\" or \"adjusting\" above"
        }
        : "Renew";

    private int _adjustOrCorrect;

    public int AdjustOrCorrect {
      get =>
        _adjustOrCorrect;
      set {
        _adjustOrCorrect = value;
        if (value == AdjustOrCorrectOption_AdjustId) {
          Start = OriginalStart;
          End = OriginalEnd;
        }
      }
    }

    public const int AdjustOrCorrectOption_CorrectId = 1;
    public const int AdjustOrCorrectOption_AdjustId = 2;

    public List<ItemModel> AdjustOrCorrectOptions = new() {
      new() { Id = AdjustOrCorrectOption_CorrectId, Text = "correcting" },
      new() { Id = AdjustOrCorrectOption_AdjustId, Text = "adjusting" }
    };

    public int CurrentTermId { get; set; }

    public DateTime MinStart { get; set; }
    public DateTime OriginalStart { get; set; }

    private DateTime _start;

    public DateTime Start {
      get =>
        _start;
      set =>
        _start = value.StartOfMonth();
    }

    public DateTime OriginalEnd { get; set; }

    private DateTime _end;

    public DateTime End {
      get =>
        _end;
      set =>
        _end = value.EndOfMonth().EndOfDay();
    }

    public List<LevelAmount> LevelAmounts { get; set; }
    public ObservableCollection<(int val, string name)> LevelsOvs { get; set; }
    public ObservableCollection<(int val, string name)> LevelAmountsOvs { get; set; }
    public int SelectedLevelAmountId { get; set; }
    public Frequency Frequency { get; set; }
    public PaymentMethods DefaultPaymentMethod { get; set; }
    public int DefaultPaymentSourceId { get; set; }
    public int MaxSharesPerAvreich { get; set; }
    public ObservableCollection<Share> Shares { get; set; }
    public ObservableCollection<Share> OtherShares { get; set; }

    public string NewAvreichMessage =>
      NewAvreichId == 0
        ? ""
        : NewAvreichQty == 0
          ? "Choose the # of shares"
          : "Click the Add button";
    public int NewAvreichId { get; set; }
    public int NewAvreichQty { get; set; }
    public ObservableCollection<AvreichOverview> AvailableAvreichim { get; set; }

    public void AddAvreich() {
      AvreichOverview avreich = AvailableAvreichim.Single(a => a.Id == NewAvreichId);
      Shares.Add(new() {
        AvreichId = NewAvreichId,
        Avreich = new() {
          Id = avreich.Id, 
          FirstName = avreich.FirstName,
          Surname = avreich.Surname,
          HebrewName = avreich.HebrewName
        },
        Quantity = NewAvreichQty
      });
      AvailableAvreichim.Remove(avreich);
      NewAvreichId = 0;
      NewAvreichQty = 0;
    }
  }
}