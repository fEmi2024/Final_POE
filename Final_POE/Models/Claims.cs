using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_POE.Models
{
    public enum ClaimStatus { Pending, Approved, Rejected }
    public class Claims
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClaimId { get; set; } // The standard unique identifier

        [Required]
        [EmailAddress]
        public required string LecturerEmail { get; set; } // Fully specified Lecturer Email

        [Required]
        public required string ClaimTitle { get; set; } // Fully specified Claim Title

        // --- Lecturer View Automation (F1) ---

        [Required]
        [Range(0.1, 400, ErrorMessage = "Hours claimed must be between 0.1 and 400.")]
        public decimal HoursClaimed { get; set; }

        // F1: Input field for the rate, used in auto-calculation
        [Required]
        [Range(50, 1000, ErrorMessage = "Hourly rate must be between 50 and 1000.")]
        public decimal HourlyRate { get; set; }

        // F1: Output of the auto-calculation (Hours * Rate)
        public decimal FinalPaymentAmount { get; set; } = 0.00M;

       

        // Fully specified Contract Reference
        public string ContractReference { get; set; } = string.Empty;

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        // F2: Coordinator/Manager Automation Flag
        public bool RequiresManagerFlag { get; set; } = false;

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    }
}
