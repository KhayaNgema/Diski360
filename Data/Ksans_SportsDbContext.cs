using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyField.Models;

namespace MyField.Data
{
    public class Ksans_SportsDbContext : IdentityDbContext
    {
        public Ksans_SportsDbContext(DbContextOptions<Ksans_SportsDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Standing>().HasKey(c => c.StandingId);
            modelBuilder.Entity<Club>().HasKey(c => c.ClubId);
            modelBuilder.Entity<Fixture>().HasKey(f => f.FixtureId);
            modelBuilder.Entity<MatchResults>().HasKey(r => r.ResultsId);
            modelBuilder.Entity<SportNews>().HasKey(n => n.NewsId);
            modelBuilder.Entity<PlayerTransferMarket>().HasKey(n => n.PlayerTransferMarketId);
            modelBuilder.Entity<Transfer>().HasKey(n => n.TransferId);

            modelBuilder.Entity<ClubAdministrator>().HasBaseType<UserBaseModel>();
            modelBuilder.Entity<ClubManager>().HasBaseType<UserBaseModel>();
            modelBuilder.Entity<Player>().HasBaseType<UserBaseModel>();

            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.HomeTeam)
                .WithMany()
                .HasForeignKey(f => f.HomeTeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.AwayTeam)
                .WithMany()
                .HasForeignKey(f => f.AwayTeamId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Club>()
                .HasOne(c => c.ClubManager)
                .WithOne(cm => cm.Club)
                .HasForeignKey<ClubManager>(cm => cm.ClubId)
                .HasPrincipalKey<Club>(c => c.ClubId)
                .IsRequired(false);

            modelBuilder.Entity<ClubAdministrator>()
                .HasOne(ca => ca.Club)
                .WithMany()
                .HasForeignKey(ca => ca.ClubId)
                .IsRequired(true); 

            modelBuilder.Entity<UserBaseModel>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Player>("Player")
                .HasValue<ClubAdministrator>("ClubAdministrator")
                .HasValue<ClubManager>("ClubManager");

            modelBuilder.Entity<MatchFormation>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<MatchFormation>("MatchFormation")
                .HasValue<MatchFormation_Archive>("MatchFormation_Archive");

            modelBuilder.Entity<PlayerTransferMarket>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<PlayerTransferMarket>("PlayerTransferMarket");


            modelBuilder.Entity<LineUp>()
                .HasOne(l => l.Fixture)
                .WithMany()
                .HasForeignKey(l => l.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Fixture>()
                .HasIndex(f => f.KickOffDate);

            modelBuilder.Entity<LiveYellowCardHolder>()
                .Property(e => e.YellowCardReason)
                .HasConversion(
                    v => v.ToString(),
                    v => (YellowCardReason)Enum.Parse(typeof(YellowCardReason), v)
                );

            modelBuilder.Entity<LiveRedCardHolder>()
                .Property(e => e.RedCardReason)
                .HasConversion(
                    v => v.ToString(),
                    v => (RedCardReason)Enum.Parse(typeof(RedCardReason), v)
                );


            modelBuilder.Entity<TournamentClubs>()
                .HasKey(tc => new { tc.ClubId, tc.TournamentId });

            modelBuilder.Entity<TournamentClubs>()
                .Property(tc => tc.ClubId)
                .ValueGeneratedOnAdd(); 

            modelBuilder.Entity<TournamentClubs>()
                .Property(tc => tc.TournamentId)
                .ValueGeneratedNever();
        }


        public DbSet<MyField.Models.Club> Club { get; set; } = default!;
        public DbSet<MyField.Models.Standing> Standing { get; set; } = default!;
        public DbSet<MyField.Models.SportNews> SportNew { get; set; } = default!;
        public DbSet<MyField.Models.Fixture> Fixture { get; set; } = default!;
        public DbSet<MyField.Models.MatchResults> MatchResult { get; set; } = default!;

        public DbSet<MyField.Models.UserBaseModel> SystemUsers { get; set; } = default!;

        public DbSet<MyField.Models.Tournament> Tournament { get; set; } = default!;

        public DbSet<MyField.Models.HeadTohead> HeadToHead { get; set; } = default!;

        public DbSet<MyField.Models.Meeting> Meeting { get; set; } = default!;

        public DbSet<MyField.Models.LineUpXIHolder> LineUpXIHolder { get; set; } = default!;

        public DbSet<MyField.Models.LineUpXI> LineUpXI { get; set; } = default!;

        public DbSet<MyField.Models.LineUpSubstitutesHolder> LineUpSubstitutesHolder { get; set; } = default!;

        public DbSet<MyField.Models.LineUpSubstitutes> LineUpSubstitutes { get; set; } = default!;

        public DbSet<MyField.Models.LineUp> LineUp { get; set; } = default!;

        public DbSet<MyField.Models.Player> Player { get; set; } = default!;
        public DbSet<MyField.Models.ClubAdministrator> ClubAdministrator { get; set; } = default!;

        public DbSet<MyField.Models.ClubManager> ClubManager { get; set; } = default!;

        public DbSet<MyField.Models.SystemAdministrator> SystemAdministrator{ get; set; } = default!;

        public DbSet<MyField.Models.SportsMember> SportMember { get; set; } = default!;

        public DbSet<MyField.Models.Officials> Officials { get; set; } = default!;

        public DbSet<MyField.Models.MatchOfficials> MatchOfficials { get; set; } = default!;

        public DbSet<MyField.Models.Formation> Formations { get; set; } = default!;

        public DbSet<MyField.Models.MatchFormation> MatchFormation{ get; set; } = default!;

        public DbSet<MyField.Models.Comment> Comments { get; set; } = default!;
        public DbSet<MyField.Models.Fine> Fines { get; set; } = default!;

        public DbSet<MyField.Models.Maintainance> Maintainances { get; set; } = default!;

        public DbSet<MyField.Models.Fan> Fans { get; set; } = default!;

        public DbSet<MyField.Models.ClubWarning> ClubWarnings { get; set; } = default!;

        public DbSet<MyField.Models.Warning> Warnings { get; set; } = default!;

        public DbSet<MyField.Models.League> League { get; set; } = default!;

        public DbSet<MyField.Models.Standings_Archive> Standings_Archive { get; set; } = default!;

        public DbSet<MyField.Models.MatchResults_Archive> MatchResults_Archive { get; set; } = default!;

        public DbSet<MyField.Models.Fixtures_Archive> Fixtures_Archive { get; set; } = default!;

        public DbSet<MyField.Models.Clubs_Archive> Clubs_Archive { get; set; } = default!;

        public DbSet<MyField.Models.Transfer> Transfer { get; set; } = default!;

        public DbSet<MyField.Models.PlayerTransferMarket> PlayerTransferMarket { get; set; } = default!;

        public DbSet<MyField.Models.Payment> Payments { get; set; } = default!;

        public DbSet<MyField.Models.Invoice> Invoices { get; set; } = default!;

        public DbSet<MyField.Models.DeviceInfo> DeviceInfo { get; set; } = default!;

        public DbSet<MyField.Models.PlayerTransferMarketArchive> PlayerTransferMarketArchive { get; set; } = default!;

        public DbSet<MyField.Models.ActivityLog> ActivityLogs { get; set; } = default!;

        public DbSet<MyField.Models.UserBaseModel> UserBaseModel { get; set; } = default!;

        public DbSet<MyField.Models.TransferPeriod> TransferPeriod { get; set; } = default!;

        public DbSet<MyField.Models.MatchFormation_Archive> MatchFormation_Archive { get; set; } = default!;

        public DbSet<MyField.Models.Reports> Reports { get; set; } = default!;

        public DbSet<MyField.Models.MatchReports> MatchReports { get; set; } = default!;

        public DbSet<MyField.Models.TransfersReports> TransfersReports { get; set; } = default!;

        public DbSet<MyField.Models.MatchResultsReports> MatchResultsReports { get; set; } = default!;

        public DbSet<MyField.Models.MatchReports_Archive> MatchReportsArchive { get; set; } = default!;

        public DbSet<MyField.Models.MatchResultsReports_Archive> MatchResultsReports_Archive { get; set; } = default!;

        public DbSet<MyField.Models.TransfersReports_Archive> TransfersReports_Archive { get; set; } = default!;
        public DbSet<MyField.Models.ClubPerformanceReport> ClubPerformanceReports { get; set; } = default!;
        public DbSet<MyField.Models.ClubTransferReport> ClubTransferReports { get; set; } = default!;

        public DbSet<MyField.Models.ClubTransferReports_Archive> ClubTransferReports_Archive { get; set; } = default!;
        public DbSet<MyField.Models.ClubPerformanceReports_Archive> ClubPerformanceReports_Archive { get; set; } = default!;

        public DbSet<MyField.Models.TestUserFeedback> TestUserFeedbacks { get; set; } = default!;

        public DbSet<MyField.Models.OverallNewsReport> OverallNewsReports { get; set; } = default!;

        public DbSet<MyField.Models.IndividualNewsReport> IndividualNewsReports { get; set; } = default!;

        public DbSet<MyField.Models.PersonnelAccountsReport> PersonnelAccountsReports { get; set; } = default!;

        public DbSet<MyField.Models.PersonnelFinancialReport> PersonnelFinancialReports{ get; set; } = default!;
        public DbSet<MyField.Models.FansAccountsReport> FansAccountsReports { get; set; } = default!;

        public DbSet<MyField.Models.Announcement> Announcements { get; set; } = default!;

        public DbSet<MyField.Models.TermsAggreement> TermsAggreements { get; set; } = default!;

        public DbSet<MyField.Models.CookiePreferences> CookiePreferences { get; set; } = default!;

        public DbSet<MyField.Models.Live> Live { get; set; } = default!;

        public DbSet<MyField.Models.LiveGoal> LiveGoals { get; set; } = default!;

        public DbSet<MyField.Models.LiveGoalHolder> LiveGoalHolders { get; set; } = default!;

        public DbSet<MyField.Models.YellowCard> YellowCards { get; set; } = default!;

        public DbSet<MyField.Models.RedCard> RedCards { get; set; } = default!;

        public DbSet<MyField.Models.Penalty> Penalties { get; set; } = default!;

        public DbSet<MyField.Models.TopScore> TopScores { get; set; } = default!;

        public DbSet<MyField.Models.TopAssist> TopAssists { get; set; } = default!;

        public DbSet<MyField.Models.Substitute> Substitutes { get; set; } = default!;


        public DbSet<MyField.Models.LiveRedCardHolder> LiveRedCardHolders { get; set; } = default!;

        public DbSet<MyField.Models.LiveYellowCardHolder> LiveYellowCardHolders { get; set; } = default!;

        public DbSet<MyField.Models.PlayerPerformanceReport> PlayerPerformanceReports { get; set; } = default!;

        public DbSet<MyField.Models.PlayerPerformanceReports_Archive> PlayerPerformanceReports_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Live_Archive> Live_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LiveGoals_Archive> LiveGoals_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LiveYellowCard_Archive> YellowCard_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LiveRedCard_Archive> LiveRedCard_Archives { get; set; } = default!;

        public DbSet<MyField.Models.PlayerTransferMarket_Archive> PlayerTransferMarket_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Transfer_Archive> Transfer_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Invoices_Archive> Invoices_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Penalty_Archive> Penalty_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Substitute_Archive> Substitute_Archives { get; set; } = default!;


        public DbSet<MyField.Models.TopAssists_Archive> TopAssists_Archives { get; set; } = default!;

        public DbSet<MyField.Models.TopScores_Archive> TopScores_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Event> LiveEvents { get; set; } = default!;

        public DbSet<MyField.Models.LineUps_Archive> LineUps_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LineUpXI_Archive> LineUpXI_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LineUpSubstitutes_Archive> LineUpSubstitutes_Archives { get; set; } = default!;

        public DbSet<MyField.Models.LiveOwnGoalHolder> LiveOwnGoalHolders { get; set; } = default!;


        public DbSet<MyField.Models.OwnGoals_Archive> OwnGoals_Archives { get; set; } = default!;

        public DbSet<MyField.Models.MatchOfficials_Archive> MatchOfficials_Archives { get; set; } = default!;


        public DbSet<MyField.Models.LiveEvents_Archives> LiveEvents_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Goals_Archive> Goals_Archives { get; set; } = default!;


        public DbSet<MyField.Models.Yellow_Archive> Yellow_Archives { get; set; } = default!;


        public DbSet<MyField.Models.Red_Archive> Red_Archives { get; set; } = default!;

        public DbSet<MyField.Models.Division> Divisions { get; set; } = default!;

        public DbSet<MyField.Models.DivisionManager> DivisionManagers { get; set; } = default!;

        public DbSet<MyField.Models.OnboardingRequest> OnboardingRequests { get; set; } = default!;

        public DbSet<MyField.Models.DivisionAggreement> DivisionAggreements { get; set; } = default!;

        public DbSet<MyField.Models.OnboardingRequestsReports> OnboardingRequestsReports { get; set; } = default!;

        public DbSet<MyField.Models.UserAccountsReports> UserAccountsReports { get; set; } = default!;

        public DbSet<MyField.Models.TransactionsReports> TransactionsReports { get; set; } = default!;

        public DbSet<MyField.Models.SystemPerformanceReport> SystemPerformanceReports { get; set; } = default!;

        public DbSet<MyField.Models.RequestLog> RequestLogs { get; set; } = default!;

        public DbSet<MyField.Models.Subscription> Subscriptions { get; set; } = default!;

        public DbSet<MyField.Models.PayFastWebhookPayload> PayFastWebhookRecords { get; set; }

        public DbSet<MyField.Models.SubscriptionHistory> SubscriptionHistories { get; set; }

        
        public DbSet<MyField.Models.Competition> Competition { get; set; }

        public DbSet<MyField.Models.CompetitionParticipants> CompetitionParticipants { get; set; }

        public DbSet<MyField.Models.UserManuals> UserManuals { get; set; }

        public DbSet<MyField.Models.TournamentRules> TournamentRules { get; set; }

        public DbSet<MyField.Models.TournamentClubs> TournamentClubs { get; set; }

        public DbSet<MyField.Models.TournamentFixture> TournamentFixtures { get; set; }

        public DbSet<MyField.Models.TournamentMatchResults> TournamentMatchResults { get; set; }

    }
}
