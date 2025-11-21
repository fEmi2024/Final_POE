using Final_POE.Data;
using Final_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_POE.Services
{
    public class ClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        // F1 & F3: Submit Claim (Auto-Calculation and Flagging)
        public async Task<Claims> SubmitClaim(Claims newClaim)
        {
            // F1: Auto-Calculation Implementation
            newClaim.FinalPaymentAmount = newClaim.HoursClaimed * newClaim.HourlyRate;

            // F2: Automation: Flag claims over 160 hours
            if (newClaim.HoursClaimed > 160)
            {
                newClaim.RequiresManagerFlag = true;
            }
            else
            {
                newClaim.RequiresManagerFlag = false;
            }

            newClaim.Status = ClaimStatus.Pending;
            newClaim.SubmissionDate = DateTime.UtcNow;

            _context.Claims.Add(newClaim);
            await _context.SaveChangesAsync();

            return newClaim;
        }

        // F2 & P3: Get Verification Queue (Priority Queuing)
        public async Task<List<Claims>> GetVerificationQueue()
        {
            return await _context.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                // Priority Queuing: Flagged claims appear first
                .OrderByDescending(c => c.RequiresManagerFlag)
                .ThenBy(c => c.SubmissionDate)
                .ToListAsync();
        }

        // F2: Process Decision
        public async Task<bool> ProcessClaimDecision(int claimId, ClaimStatus status)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null || claim.Status != ClaimStatus.Pending) return false;

            claim.Status = status;
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return true;
        }

        // P3: HR Automation - Calculate Approved Payroll Hours
        public async Task<decimal> CalculateApprovedPayrollHours()
        {
            return await _context.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .SumAsync(c => c.HoursClaimed);
        }

        // F4: Get Lecturer's Claims
        public async Task<List<Claims>> GetLecturerClaims(string email)
        {
            return await _context.Claims
                .Where(c => c.LecturerEmail == email)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }
    }
}
