using Xunit;
using Microsoft.EntityFrameworkCore;
using Final_POE.Data;
using Final_POE.Models;
using Final_POE.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Final_POE.Tests
{
    public class ClaimServiceTests
    {
        private ApplicationDbContext GetDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        // Test 1 (No changes needed, as service.SubmitClaim guarantees a non-null return)
        [Fact]
        public async Task SubmitClaim_SetsCorrectFlagAndCalculatesPayment()
        {
            // Arrange
            var context = GetDbContext("F1F2_TestDB");
            var service = new ClaimService(context);

            var lowHoursClaim = new Claims
            {
                LecturerEmail = "lecturer@test.com",
                ClaimTitle = "Standard Month Claim",
                HoursClaimed = 150.00M,
                HourlyRate = 100.00M,
                ContractReference = "CTR-001"
            };
            var highHoursClaim = new Claims
            {
                LecturerEmail = "manager@test.com",
                ClaimTitle = "Overtime Project Claim",
                HoursClaimed = 160.01M,
                HourlyRate = 200.00M,
                ContractReference = "CTR-002"
            };

            // Act
            var submittedLow = await service.SubmitClaim(lowHoursClaim);
            var submittedHigh = await service.SubmitClaim(highHoursClaim);

            // Assert
            Assert.False(submittedLow.RequiresManagerFlag);
            Assert.True(submittedHigh.RequiresManagerFlag);
            Assert.Equal(15000.00M, submittedLow.FinalPaymentAmount);
            Assert.Equal(32002.00M, submittedHigh.FinalPaymentAmount);
            Assert.Equal(ClaimStatus.Pending, submittedLow.Status);
        }

        // Test 2 (No changes needed)
        [Fact]
        public async Task GetVerificationQueue_OrdersByPriorityFlag()
        {
            // Arrange
            var context = GetDbContext("P3_QueueTestDB");
            var service = new ClaimService(context);

            context.Claims.AddRange(new List<Claims>
            {
                new Claims { ClaimId = 1, HoursClaimed = 100, RequiresManagerFlag = false, Status = ClaimStatus.Pending, SubmissionDate = DateTime.UtcNow.AddMinutes(1), LecturerEmail = "a@a.com", ClaimTitle="Low Hours" },
                new Claims { ClaimId = 2, HoursClaimed = 200, RequiresManagerFlag = true, Status = ClaimStatus.Pending, SubmissionDate = DateTime.UtcNow, LecturerEmail = "b@b.com", ClaimTitle="High Hours 1" },
                new Claims { ClaimId = 3, HoursClaimed = 170, RequiresManagerFlag = true, Status = ClaimStatus.Pending, SubmissionDate = DateTime.UtcNow.AddMinutes(2), LecturerEmail = "c@c.com", ClaimTitle="High Hours 2" },
                new Claims { ClaimId = 4, HoursClaimed = 50, RequiresManagerFlag = false, Status = ClaimStatus.Approved, SubmissionDate = DateTime.UtcNow.AddMinutes(3), LecturerEmail = "d@d.com", ClaimTitle="Approved" },
            });
            await context.SaveChangesAsync();

            // Act
            var queue = await service.GetVerificationQueue();

            // Assert
            Assert.Equal(3, queue.Count);
            Assert.Equal(2, queue[0].ClaimId);
            Assert.Equal(3, queue[1].ClaimId);
            Assert.Equal(1, queue[2].ClaimId);
        }

        // Test 3 (No changes needed)
        [Fact]
        public async Task CalculateApprovedPayrollHours_SumsOnlyApprovedClaims()
        {
            // Arrange
            var context = GetDbContext("P3_PayrollTestDB");
            var service = new ClaimService(context);

            context.Claims.AddRange(new List<Claims>
            {
                new Claims { ClaimId = 1, HoursClaimed = 100, Status = ClaimStatus.Approved, LecturerEmail = "a@a.com", ClaimTitle="Approved 1" },
                new Claims { ClaimId = 2, HoursClaimed = 50.5M, Status = ClaimStatus.Approved, LecturerEmail = "b@b.com", ClaimTitle="Approved 2" },
                new Claims { ClaimId = 3, HoursClaimed = 200, Status = ClaimStatus.Rejected, LecturerEmail = "c@c.com", ClaimTitle="Rejected" },
                new Claims { ClaimId = 4, HoursClaimed = 25, Status = ClaimStatus.Pending, LecturerEmail = "d@d.com", ClaimTitle="Pending" },
            });
            await context.SaveChangesAsync();

            // Act
            var totalHours = await service.CalculateApprovedPayrollHours();

            // Assert
            Assert.Equal(150.5M, totalHours);
        }

        // -------------------------------------------------------------------
        // Test 4: F2 (Process Decision) Logic - FIX APPLIED HERE
        // -------------------------------------------------------------------
        [Fact]
        public async Task ProcessClaimDecision_UpdatesStatusCorrectly()
        {
            // Arrange
            var context = GetDbContext("F2_DecisionTestDB");
            var service = new ClaimService(context);

            var pendingClaim = new Claims
            {
                ClaimId = 101,
                LecturerEmail = "test@test.com",
                ClaimTitle = "Pending Claim",
                HoursClaimed = 100,
                HourlyRate = 100,
                Status = ClaimStatus.Pending
            };
            context.Claims.Add(pendingClaim);
            await context.SaveChangesAsync();

            // Act 1: Approve the claim
            bool successApprove = await service.ProcessClaimDecision(101, ClaimStatus.Approved);

            // Assert 1: Approval success
            Assert.True(successApprove);
            var approvedClaim = await context.Claims.FindAsync(101);

            // FIX: Assert that the object was retrieved successfully (not null)
            Assert.NotNull(approvedClaim);

            // Use the null-forgiving operator (!) or cast to avoid the CS8602 warning
            Assert.Equal(ClaimStatus.Approved, approvedClaim!.Status);

            // Act 2: Try to reject the claim again (should fail because it's no longer Pending)
            bool successRejectFailed = await service.ProcessClaimDecision(101, ClaimStatus.Rejected);

            // Assert 2: Rejection attempt should fail
            Assert.False(successRejectFailed);
        }
    }
}