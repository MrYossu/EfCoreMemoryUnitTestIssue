using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UnitTestIssue.Models {
  public class AppDbContext : IdentityDbContext<Investor> {
    public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options) {
    }

    public DbSet<Avreich> Avreichim { get; set; }
    public DbSet<AvreichPayment> AvreichPayments { get; set; }
    public DbSet<Investor> Investors { get; set; }
    public DbSet<MaxSharesAtDate> MaxSharesAtDates { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<InvestorPayment> InvestorPayments { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<LevelAmount> LevelAmounts { get; set; }
    public DbSet<PaymentSource> PaymentSources { get; set; }
    public DbSet<Share> Shares { get; set; }
    public DbSet<StatementInformation> StatementInformations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<Level>()
        .HasData(
          new() { Id = 1, Name = "Syndicate" },
          new() { Id = 2, Name = "Business" },
          new() { Id = 3, Name = "Corporate" },
          new() { Id = 4, Name = "Premier" }
        );
      modelBuilder.Entity<LevelAmount>().HasData(
        new LevelAmount { Id = 1, LevelId = 1, Amount = 50, NumberOfShares = 1, GoCardlessLink = "https://pay.gocardless.com/AL0001Q9RAA6NP" },
        new LevelAmount { Id = 2, LevelId = 1, Amount = 100, NumberOfShares = 2, GoCardlessLink = "https://pay.gocardless.com/AL0001QCW8G6WZ" },
        new LevelAmount { Id = 3, LevelId = 1, Amount = 150, NumberOfShares = 3, GoCardlessLink = "https://pay.gocardless.com/AL0001QCWBH3HS" },
        new LevelAmount { Id = 4, LevelId = 1, Amount = 200, NumberOfShares = 4, GoCardlessLink = "https://pay.gocardless.com/AL0001QCWCF8T2" },
        new LevelAmount { Id = 5, LevelId = 1, Amount = 300, NumberOfShares = 6, GoCardlessLink = "https://pay.gocardless.com/AL0001QCWF1MQ6" },
        new LevelAmount { Id = 6, LevelId = 2, Amount = 200, NumberOfShares = 4, GoCardlessLink = "https://pay.gocardless.com/AL0001QCXEYP6T" },
        new LevelAmount { Id = 7, LevelId = 2, Amount = 300, NumberOfShares = 6, GoCardlessLink = "https://pay.gocardless.com/AL0001QCX7FRT3" },
        new LevelAmount { Id = 9, LevelId = 3, Amount = 600, NumberOfShares = 12, GoCardlessLink = "https://pay.gocardless.com/AL0001Q9S9Z9FB" },
        new LevelAmount { Id = 11, LevelId = 4, Amount = 750, NumberOfShares = 15, GoCardlessLink = "" },
        new LevelAmount { Id = 12, LevelId = 4, Amount = 900, NumberOfShares = 18, GoCardlessLink = "" },
        new LevelAmount { Id = 13, LevelId = 4, Amount = 1050, NumberOfShares = 21, GoCardlessLink = "" },
        new LevelAmount { Id = 14, LevelId = 4, Amount = 1200, NumberOfShares = 24, GoCardlessLink = "" },
        new LevelAmount { Id = 15, LevelId = 4, Amount = 1350, NumberOfShares = 27, GoCardlessLink = "" },
        new LevelAmount { Id = 16, LevelId = 4, Amount = 1500, NumberOfShares = 30, GoCardlessLink = "" },
        new LevelAmount { Id = 17, LevelId = 4, Amount = 1650, NumberOfShares = 33, GoCardlessLink = "" },
        new LevelAmount { Id = 18, LevelId = 4, Amount = 1800, NumberOfShares = 36, GoCardlessLink = "" }
      );
      modelBuilder.Entity<MaxSharesAtDate>().HasData(
        new MaxSharesAtDate { Id = 1, Date = new(1900, 1, 1), MaxShares = 12 }
      );
      modelBuilder.Entity<PaymentSource>().HasData(
        new PaymentSource { Id = -1, Name = "Unknown" }
      );
      modelBuilder.Entity<StatementInformation>().HasData(
        new StatementInformation { Id = 1, ReportText = "<p>@AVREICHLEARNS@ at Kollel Tiferes Chaim, well-known for its high standard of learning and Hasmoda.</p><p>During the morning Seder the Kollel is currently involved in the Sugyos of Meleches Shabbos, after having thoroughly learnt through the complicated Sugyos of Hoitzo’oh, Mukze and Sh’gogos. The other areas of the Mesechta are learnt during the afternoon Seder. The Kollel look forward to completing the Mesechta at the end of the coming Summer Zman</p><p>The Kollel has recently started a Sunday Morning Chabura for members of the public, where guided Iyun is provided in an independent Sugya each week. The Chabura begins with solid Chavrusa learning and is followed by a Shiur from one of the Bnei HaKollel.</p><p>To date, the Chabura has gone through six different Sugyas regarding Chanuka, a further six Sugyas regarding Hadlokas Neiros Shabbos and has now commenced a series of four Sugyas regarding Purim. For more information, to join or to start a Chabura in your shul, please email <a href=\"mailto:info@achim.org.uk\">info@achim.org.uk</a> or directly at <a href=\"mailto:chabura@chayeinu.co.uk\">chabura@chayeinu.co.uk</a>.</p>" }
      );
    }
  }
}