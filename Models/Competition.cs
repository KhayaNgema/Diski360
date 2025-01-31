using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class CompetitionParticipants
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompetitionParticipantsId { get; set; }

        public int CompetitionId { get; set; }

        public virtual Competition Competition { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserBaseModel Participant { get; set; }

        public int Points { get; set; }
    }

    public class Competition
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int CompetitionId { get; set; }

        public DateTime Month {  get; set; }

        public string? WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public virtual UserBaseModel Winner { get; set; }

        public int NumberOfParticipants { get; set; }

        public CompetitionStatus CompetitionStatus { get; set; }

    }


    public enum CompetitionStatus
    {
        Current,
        Ended
    }
}
