using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using UnitTestIssue.Models;

namespace UnitTestIssue.Tests {
  public static class TestData {
    #region AppDbContext

    public static async Task<AppDbContext> GetAppDbContext() {
      // Need this to avoid too many services being added
      ServiceProvider serviceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();
      DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("Achim", new InMemoryDatabaseRoot())
        .UseInternalServiceProvider(serviceProvider)
        .Options;
      AppDbContext appDbContext = new(options);
      await appDbContext.Levels.AddRangeAsync(Levels);
      await appDbContext.SaveChangesAsync();
      return appDbContext;
    }

    #endregion

    #region Levels and level amounts

    public const int Level_Syndicate = 1;
    public const int LevelAmount_Syndicate50 = 1;
    public const int LevelAmount_Syndicate100 = 2;
    public const int LevelAmount_Syndicate150 = 3;
    public const int Level_Business = 2;
    public const int LevelAmount_Business100 = 4;
    public const int LevelAmount_Business200 = 5;
    public const int LevelAmount_Business300 = 6;
    public const int LevelAmount_Premiere750 = 750;
    public const int LevelAmount_Premiere900 = 900;
    public const int LevelAmount_Premiere1150 = 1150;
    public const int LevelAmount_Premiere1200 = 1200;
    public const int LevelAmount_Premiere1350 = 1350;
    public const int LevelAmount_Premiere1500 = 1500;
    public const int LevelAmount_Premiere1650 = 1650;
    public const int LevelAmount_Premiere1800 = 1800;
    public const int Level_Full = 3;
    public const int Level_Premiere = 4;
    public const int LevelAmount_Corporate600 = 7;

    public static List<Level> Levels = new() {
      new() {
        Id = Level_Syndicate, Name = "Syndicate", Amounts = new() {
          new() { Id = LevelAmount_Syndicate50, Amount = 50, NumberOfShares = 1, LevelId = Level_Syndicate },
          new() { Id = LevelAmount_Syndicate100, Amount = 100, NumberOfShares = 2, LevelId = Level_Syndicate },
          new() { Id = LevelAmount_Syndicate150, Amount = 150, NumberOfShares = 3, LevelId = Level_Syndicate },
        }
      },
      new() {
        Id = 2, Name = "Business", Amounts = new() {
          new() { Id = LevelAmount_Business100, Amount = 100, NumberOfShares = 2, LevelId = Level_Business },
          new() { Id = LevelAmount_Business200, Amount = 200, NumberOfShares = 4, LevelId = Level_Business },
          new() { Id = LevelAmount_Business300, Amount = 300, NumberOfShares = 6, LevelId = Level_Business },
        }
      },
      new() {
        Id = 3, Name = "Corporate", Amounts = new() {
          new() { Id = LevelAmount_Corporate600, Amount = 600, NumberOfShares = 12, LevelId = Level_Full },
        }
      },
      new() {
        Id = 4, Name = "Premiere", Amounts = new() {
          new() { Id = LevelAmount_Premiere750, Amount = 750, NumberOfShares = 15, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere900, Amount = 900, NumberOfShares = 18, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1150, Amount = 1150, NumberOfShares = 21, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1200, Amount = 1200, NumberOfShares = 24, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1350, Amount = 1350, NumberOfShares = 27, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1500, Amount = 1500, NumberOfShares = 30, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1650, Amount = 1650, NumberOfShares = 33, LevelId = Level_Premiere },
          new() { Id = LevelAmount_Premiere1800, Amount = 1800, NumberOfShares = 36, LevelId = Level_Premiere },
        }
      }
    };

    #endregion

    #region Payment sources

    public const int PaymentSource_Bank = 1;
    public const int PaymentSource_StandingOrder = 2;
    public const int PaymentSource_Voucher = 3;

    #endregion
  }
}