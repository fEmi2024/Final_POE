
using Final_POE.Models;
using Final_POE.Services;
using Microsoft.AspNetCore.Mvc;

namespace Final_POE.Controllers
{
    public class ClaimMvcController : Controller
    {
            private readonly ClaimService _claimService;

            public ClaimMvcController(ClaimService claimService)
            {
                _claimService = claimService;
            }

        // F1 & F3: Submit Claim (GET and POST)
        [HttpGet]
        // FIX: Fully qualify 'Claim' to ensure the custom model is used.
        public IActionResult Submit() => View(new Final_POE.Models.ClaimStatus());

        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: Fully qualify 'Claim' here as well.
        public async Task<IActionResult> Submit(Final_POE.Models.Claims claim)
        {
                if (ModelState.IsValid)
                {
                    await _claimService.SubmitClaim(claim);
                    TempData["SuccessMessage"] = "Claim successfully submitted for verification.";
                    return RedirectToAction(nameof(MyClaims), new { email = claim.LecturerEmail });
                }
                return View(claim);
            }

            // F2 & P3: Verification Queue
            [HttpGet]
            public async Task<IActionResult> VerificationQueue()
            {
                var queue = await _claimService.GetVerificationQueue();
                return View(queue);
            }

            // F2: Process Decision
            [HttpPost]
            public async Task<IActionResult> ProcessDecision(int claimId, ClaimStatus status)
            {
                if (status != ClaimStatus.Approved && status != ClaimStatus.Rejected)
                {
                    TempData["ErrorMessage"] = "Invalid status provided.";
                    return RedirectToAction(nameof(VerificationQueue));
                }

                bool success = await _claimService.ProcessClaimDecision(claimId, status);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Claim {claimId} was successfully set to {status}.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to process claim {claimId}.";
                }

                return RedirectToAction(nameof(VerificationQueue));
            }

            // P3: Payroll Report
            [HttpGet]
            public async Task<IActionResult> PayrollReport()
            {
                var totalHours = await _claimService.CalculateApprovedPayrollHours();
                return View(totalHours);
            }

            // F4: Lecturer Claims Tracking
            [HttpGet]
            public async Task<IActionResult> MyClaims(string email)
            {
                var claims = await _claimService.GetLecturerClaims(email ?? "");
                ViewData["LecturerEmail"] = email;
                return View(claims);
            }
        }
    }
