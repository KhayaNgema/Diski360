using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MyField.Models
{



    public class Live
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LiveId { get; set; }

        public int FixtureId { get; set; }

        public virtual Fixture Fixture { get; set; }

        public int LeagueId { get; set; }

        public virtual League League { get; set; }

        public int LiveTime { get; set; }

        public int HomeTeamScore { get; set; }

        public int AwayTeamScore { get; set; }

        public bool IsLive { get; set; }

        public bool IsHalfTime { get; set; }


        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public bool ISEnded { get; set; }

        public bool IsInterrupted { get; set; }

        public bool WentToHalfTime { get; set; }

        public int? AddedTime { get; set; }

        public string? HalfTimeScore { get; set; }

        public DateTime? RecordedTime { get; set; }

        public LiveStatus LiveStatus { get; set; }


        public string? ReasonForInterruption { get; set; }
    }

    public enum LiveStatus
    {
        Ongoing,
        Interrupted,
        Ended
    }

    public class Event
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        public int LiveId { get; set; }

        public virtual Live Live { get; set; }

        public int LeagueId { get; set; }

        public int? DivisionId { get; set; }

        public virtual Division Division { get; set; }

        public virtual League League { get; set; }

        public DateTime RecordedTime { get; set; }
    }


    public class LiveGoal : Event
    {
        public string ScoreById { get; set; }

        public virtual Player ScoreBy { get; set; }

        public string ScoredTime { get; set; }

        public string? AssistedById { get; set; }

        public virtual Player AssistedBy { get; set; }
    }

    public class YellowCard : Event
    {
        public string YellowCommitedById { get; set; }

        public virtual Player YellowCommitedBy{ get; set; }

        public string YellowCardTime { get; set; }

        public YellowCardReason YellowCardReason { get; set; }
    }

    public class RedCard  : Event
    {
        public string RedCommitedById { get; set; }

        public virtual Player RedCommitedBy { get; set; }
        public string RedCardTime { get; set; }

        public RedCardReason RedCardReason { get; set; }

    }

    public class Penalty : Event
    {
        public string PenaltyTime { get; set; }

        public string PlayerId { get; set; }

        public virtual Player Player { get; set; }
    }


    public class Substitute : Event
    {
        public string OutPlayerId { get; set; }

        public virtual Player OutPlayer { get; set; }

        public  string InPlayerId { get; set; }

        public virtual Player InPlayer { get; set;}

        public string SubTime { get; set; }
    }

    public class LiveGoalHolder : Event
    {
        public string ScoredById { get; set; }

        public virtual Player ScoredBy { get; set; }

        public string ScoredTime { get; set; }


        public string? AssistedById { get; set; }

        public virtual Player AssistedBy { get; set; }
    }


    public class LiveOwnGoalHolder : Event
    {
        public string OwnGoalScoredById { get; set; }

        public virtual Player OwnGoalScoredBy { get; set; }

        public string OwnGoalTime { get; set; }
    }


    public class LiveYellowCardHolder : Event
    {
        public string YellowCommitedById { get; set; }

        public virtual Player YellowCommitedBy { get; set; }

        public string YellowCardTime { get; set; }

        public YellowCardReason YellowCardReason { get; set; }
    }

    public enum YellowCardReason
    {
        [Display(Name = "Delay Restart")]
        Delay_Restart,

        [Display(Name = "Dissent")]
        Dissent,

        [Display(Name = "Unauthorized Entry")]
        Unauthorized_Entry,

        [Display(Name = "Distance Violation")]
        Distance_Violation,

        [Display(Name = "Persistent Offenses")]
        Persistent_Offenses,

        [Display(Name = "Unsporting Behavior")]
        Unsporting_Behavior,
    }


    public class LiveRedCardHolder : Event  
    {
        public string RedCommitedById { get; set; }

        public virtual Player RedCommitedBy { get; set; }

        public string RedCardTime { get; set; }

        public RedCardReason RedCardReason { get; set; }
    }

    public enum RedCardReason
    {
        [Description("Handball Denial")]
        Handball_Denial,

        [Description("Foul Denial")]
        Foul_Denial,

        [Description("Serious Foul Play")]
        Serious_Foul_Play,

        [Description("Biting or Spitting")]
        Biting_Spitting,

        [Description("Violent Conduct")]
        Violent_Conduct,

        [Description("Offensive Language")]
        Offensive_Language,

        [Description("Second Yellow Card")]
        Second_Yellow
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayAttribute));
            return attribute == null ? value.ToString() : attribute.Name;
        }

        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }

}
