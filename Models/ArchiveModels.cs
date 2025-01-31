using System.ComponentModel.DataAnnotations;

namespace MyField.Models
{
    public class Standings_Archive : Standing
    {
        [Key]
        public int StandingId { get; set; }
    }

    public class MatchResults_Archive : MatchResults
    {
        [Key]
        public int ResultsId { get; set; }
    }

    public class Fixtures_Archive : Fixture
    {
        [Key]
        public int FixtureId { get; set; }
    }

    public class Clubs_Archive : Club
    {
        [Key]
        public int ClubId { get; set; }
    }

    public class MatchFormation_Archive : MatchFormation
    {
        [Key]
        public int MatchFormationId { get; set; }
    }

    public class MatchReports_Archive : MatchReports
    {
        [Key]
        public int ReportId { get; set; }
    }

    public class MatchResultsReports_Archive : MatchResultsReports
    {
        [Key]
        public int ReportId { get; set; }
    }

    public class TransfersReports_Archive : TransfersReports
    {
        [Key]
        public int ReportId { get; set; }
    }

    public class ClubTransferReports_Archive : ClubTransferReport
    {
        [Key]
        public int ReportId { get; set; }
    }


    public class ClubPerformanceReports_Archive : ClubPerformanceReport
    {
        [Key]
        public int ReportId { get; set; }
    }

    public class PlayerPerformanceReports_Archive : PlayerPerformanceReport
    {
        [Key]
        public int ReportId { get; set; }
    }

    public class Live_Archive : Live
    {
        [Key]
        public int LiveId {  get; set; }
    }

    public class LiveGoals_Archive : LiveGoalHolder
    {
        [Key]   
        public int EventId { get; set; }
    }

    public class LiveYellowCard_Archive : YellowCard
    {
        [Key]
        public int EventId { get; set; }
    }

    public class LiveRedCard_Archive : RedCard
    {
        [Key]
        public int EventId { get; set; }
    }

    public class Penalty_Archive : Penalty
    {
        [Key]
        public int EventId { get; set; }
    }

    public class Substitute_Archive : Substitute
    {
        [Key]
        public int EventId { get; set; }
    }

    public class TopScores_Archive : TopScore
    {
        [Key]
        public int TopScoreId { get; set; }
    }

    public class TopAssists_Archive : TopAssist
    {
        [Key]
        public int TopAssistId { get; set; }
    }

    public class LineUps_Archive : LineUp
    {
        [Key]
        public int LineUpId {  get; set; }  
    }

    public class LineUpXI_Archive : LineUpXI
    {
        [Key]
        public int LineUpXIId {  get; set; }
    }

    public class LineUpSubstitutes_Archive : LineUpSubstitutes
    {
        [Key]
        public int LineUpSubstituteId { get; set; }
    }

    public class OwnGoals_Archive : LiveOwnGoalHolder
    {
        [Key]   
        public int EventId { get; set; }
    }

    public class MatchOfficials_Archive : MatchOfficials
    {
        [Key]
        public int MatchOfficialsId { get; set; }
    }

    public class LiveEvents_Archives : Event
    {
        [Key]
        public int EventId { get; set; }
    }


    public class Goals_Archive : LiveGoal
    {
        [Key]
        public int EventId { get; set; }
    }

    public class Yellow_Archive : YellowCard
    {
        [Key]
        public int EventId { get; set; }
    }

    public class Red_Archive : RedCard
    {
        [Key]
        public int EventId { get; set; }
    }


    public class PlayerTransferMarket_Archive : PlayerTransferMarket
    {
        [Key]
        public int PlayerTransferMarketId { get; set; }
    }

    public class Transfer_Archive : Transfer
    {
        [Key]
        public int TransferId { get; set; }
    }

    public class Invoices_Archive : Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
    }
}