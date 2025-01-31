using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyField.Models
{
    public class Player : UserBaseModel
    {   
        public decimal MarketValue { get; set; }
        public int JerseyNumber { get; set; }   

        public DateTime StartDate { get; set; }    

        public DateTime EndDate { get; set; }

        public bool IsContractSigned { get; set; }  

        public string PlayerCard {  get; set; } 

        [Display(Name = "Club")]
        public int ClubId { get; set; }
        public virtual Club Club { get; set; }

        public Position Position { get; set; }  
        
        public bool HasPlayed { get; set; }

        public bool IsOnPitch { get; set; }

        public int? DivisionId { get; set; }
        [ForeignKey("DivisionId")]
        public virtual Division Division { get; set; }
    }

    public enum Position
    {
        [Display(Name = "Goalkeeper (GK)")]
        GK,

        [Display(Name = "Right Back (RB)")]
        RB,

        [Display(Name = "Left Back (LB)")]
        LB,

        [Display(Name = "Center Back (CB)")]
        CB,

        [Display(Name = "Right Wing Back (RWB)")]
        RWB,

        [Display(Name = "Left Wing Back (LWB)")]
        LWB,

        [Display(Name = "Central Defensive Midfielder (CDM)")]
        CDM,

        [Display(Name = "Central Midfielder (CM)")]
        CM,

        [Display(Name = "Right Midfielder (RM)")]
        RM,

        [Display(Name = "Left Midfielder (LM)")]
        LM,

        [Display(Name = "Right Winger (RW)")]
        RW,

        [Display(Name = "Left Winger (LW)")]
        LW,

        [Display(Name = "Central Attacking Midfielder (CAM)")]
        CAM,

        [Display(Name = "Center Forward (CF)")]
        CF,

        [Display(Name = "Striker (ST)")]
        ST
    }



}
