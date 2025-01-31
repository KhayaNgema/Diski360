using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class Reports
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }
    }

    public class OnboardingRequestsReports : Reports
    {
        public int OnboardingRequestsTotalCount { get; set; }

        public int ApprovedRequestsCount { get; set; }
        public double ApprovedRequestsRate { get; set; }

        public int RejectedRequestsCount { get; set; }
        public double RejectedRequestsRate { get; set; }

        public int CompletedRequestsCount { get; set; }
        public double CompletedRequestsRate { get; set; }
    }

    public class UserAccountsReports : Reports
    {
        public int TotalUserAccountsCount { get; set; }

        public int ActiveUserAccountsCount { get; set; }
        public double ActiveUserAccountsRate { get; set; }

        public int InactiveUserAccountsCount { get; set; }
        public double InactiveUserAccountsRate { get; set; }

        public int SuspendedUserAccountsCount { get; set; }
        public double SuspendedUserAccountsRate { get; set; }
    }

    public class SystemPerformanceReport : Reports
    {
        public int TotalRequests { get; set; }
        public int SucceededRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double FailureRate { get; set; }
    }

    public class TransactionsReports : Reports
    {
        public int TotalTransactionsCount { get; set; }

        public int SuccessfulPaymentsCount { get; set; }
        public double SuccessfulPaymentsRate { get; set; }

        public int UnsuccessfulPaymentsCount { get; set; }
        public double UnsuccessfulPaymentsRate { get; set; }
    }


    public class MatchReports : Reports
    {
        public int LeagueId { get; set; }

        public virtual League Season { get; set; }
        public int MatchesToBePlayedCount { get; set; }
        public int FixturedMatchesCount { get; set; }

        public int UnreleasedFixturesCount { get; set; }

        public int PlayedMatchesCounts { get; set; }

        public int PostponedMatchesCount { get; set; }

        public int InterruptedMatchesCount { get; set; }

        public decimal FixturedMatchesRate { get; set; }

        public decimal UnfixturedMatchesRate { get; set; }

        public decimal PlayedMatchesRate { get; set; }

        public decimal PostponedMatchesRate { get; set; }

        public decimal InterruptedMatchesRate { get; set; }
    }

    public class MatchResultsReports : Reports
    {
        public int LeagueId { get; set; }

        public virtual League Season { get; set; }

        public int ExpectedResultsCount { get; set; }

        public int ReleasedResultsCount { get; set; }

        public int UnreleasedResultsCount { get; set; }

        public int WinsCount { get; set; }

        public int LosesCount { get; set; }

        public int DrawsCount { get; set; }

        public decimal ReleasedResultsRate { get; set; }

        public decimal UnreleasedMatchesRate { get; set; }

        public decimal WinningRate { get; set; }


        public decimal LosingRate { get; set; }

        public decimal DrawingRate { get; set; }
    }

    public class TransfersReports : Reports
    {
        public int LeagueId { get; set; }

        public virtual League Season { get; set; }

        public int TransferPeriodId { get; set; }

        public virtual TransferPeriod TransferPeriod { get; set; }

        public int TransferMarketCount { get; set; }

        public int PurchasedPlayersCount { get; set; }

        public int DeclinedTransfersCount { get; set; }

        public decimal TranferAmount { get; set; }

        public decimal AssociationCut { get; set; }

        public decimal ClubsCut { get; set; }

        public decimal SuccessfulTranferRate { get; set; }

        public decimal UnsuccessfulTranferRate { get; set; }

        public decimal NotStartedTransferRate { get; set; }

    }

    public class ClubPerformanceReport : Reports
    {
        public int LeagueId { get; set; }

        public virtual League League { get; set; }
        public int ClubId { get; set; }

        public virtual Club Club { get; set; }

        public int GamesToPlayCount { get; set; }

        public int GamesPlayedCount { get; set; }

        public int GamesNotPlayedCount { get; set; }

        public int GamesWinCount { get; set; }

        public int GamesLoseCount { get; set; }

        public int GamesDrawCount { get; set; }

        public decimal GamesPlayedRate { get; set; }

        public decimal GamesNotPlayedRate { get; set; }

        public decimal GamesWinRate { get; set; }

        public decimal GamesLoseRate { get; set; }

        public decimal GamesDrawRate { get; set; }
    }

    public class ClubTransferReport : Reports
    {
        public int LeagueId { get; set; }

        public virtual League League { get; set; }
        public int ClubId { get; set; }

        public virtual Club Club { get; set; }
        public int OverallTransfersCount { get; set; }

        public int OutgoingTransfersCount { get; set; }

        public int IncomingTransfersCount { get; set; }

        public decimal OutgoingTransferRate { get; set; }

        public decimal IncomingTransferRate { get; set; }

        public int SuccessfulOutgoingTransfersCount { get; set; }

        public int SuccessfulIncomingTransfersCount { get; set; }

        public int RejectedOutgoingTransfersCount { get; set; }

        public int RejectedIncomingTransfersCount { get; set; }

        public int NotActionedIncomingTransferCount { get; set; }

        public int NotActionedOutgoigTransferCount { get; set; }

        public decimal SuccessfullIncomingTransferRate { get; set; }

        public decimal RejectedOutgoingTransferRate { get; set; }

        public decimal SuccessfullOutgoingTransferRate { get; set; }

        public decimal RejectedIncomingTransferRate { get; set; }

        public decimal NotActionedIncomingTransferRate { get; set; }

        public decimal NotActionedOutgoingTransferRate { get; set; }
    }

    public class OverallNewsReport : Reports
    {
        public int AuthoredNewsCount { get; set; }

        public int ApprovedNewsCount { get; set; }

        public int PublishedNewsCount { get; set; }

        public int RejectedNewsCount { get; set; }

        public int NewsReadersCount { get; set; }

        public decimal ApprovedNewsRate { get; set; }

        public decimal PublishedNewsRate { get; set; }

        public decimal RejectedNewsRate { get; set; }
    }

    public class IndividualNewsReport : Reports
    {
        public int ReadersCount { get; set; }

        [ForeignKey("SportNewsId")]
        public int SportNewsId { get; set; }

        public virtual SportNews SportNews { get; set; }
    }

    public class PersonnelAccountsReport : Reports
    {
        public int OverallAccountsCount { get; set; }

        public int ActiveAccountsCount { get; set; }

        public int InactiveAccountsCount { get; set; }

        public int SuspendedAccountsCount { get; set; }

        public decimal ActiveAccountsRate { get; set; }

        public decimal InactiveAccountsRate { get; set; }

        public decimal SuspendedAccountsRate { get; set; }
    }

    public class PersonnelFinancialReport : Reports
    {

        public decimal ExpectedRepayableAmount { get; set; }

        public decimal TotalPaidAmount { get; set; }

        public decimal TotalUnpaidAmount { get; set; }

        public int RepayableFinesCount { get; set; }

        public int PaidPaymentFinesCount { get; set; }

        public int PendingPaymentFinesCount { get; set; }

        public int OverduePaymentFineCount { get; set; }

        public decimal PaidFinesRate { get; set; }

        public decimal PendingFinesRate { get; set; }

        public decimal OverdueFinesRate { get; set; }
    }

    public class FansAccountsReport : Reports
    {
        public int OverallFansAccountsCount { get; set; }

        public int ActiveFansAccountsCount { get; set; }

        public int InactiveFansAccountsCount { get; set; }

        public int SuspendedFansAccountsCount { get; set; }

        public decimal ActiveFansAccountsRate { get; set; }

        public decimal InactiveFansAccountsRate { get; set; }

        public decimal SuspendedFansAccountsRate { get; set; }
    }

    public class PlayerPerformanceReport : Reports
    {
        public int LeagueId { get; set; }

        public virtual League League { get; set; }
        public string PlayerId { get; set; }

        public virtual Player Player { get; set; }

        public int AppearancesCount { get; set; }

        public int GoalsScoredCount { get; set; }

        public int AssistsCount { get; set; }

        public int YellowCardCount { get; set; }

        public int RedCardCount { get; set; }

        public int OwnGoalsScoredCount { get; set; }
    }

}
