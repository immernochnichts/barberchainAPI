using barberchainAPI.Data;
using barberchainAPI.Functional.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static barberchainAPI.Components.Pages.Profile;

namespace barberchainTest
{
    public class ProfileServiceTest
    {
        [Fact]
        public async Task GetAvailabilityForDayAsync_UsesPendingScheduleRequestAndBlocksOrders()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = new ProfileService(db);

            var order = new Order
            {
                Id = 10,
                AccountId = 2,
                AppointedTime = new DateTime(2025, 12, 21, 10, 0, 0),
                Status = OrderStatus.Waiting,
                OrderJobs = new List<OrderJob>
                {
                    new OrderJob
                    {
                        Job = new Job { DurationAtu = 2, Name = "BEARD TRIM", Descr = "BEARD TRIM MAAAFUCKKAAAA" }
                    }
                }
            };

            db.Orders.Add(order);

            var request = new ScheduleRequest
            {
                BarberId = 1,
                RequestDate = new DateOnly(2025, 12, 21),
                Status = ScheduleRequestStatus.Pending,
                AtuPattern = new BitArray(Enumerable.Repeat(true, 96).ToArray()),
                OrderIdsToDecline = "10",
                Message = "REQUEST MESSAGE MAAAFUCKAAA"
            };

            db.ScheduleRequests.Add(request);
            db.SaveChanges();

            var availability = await service.GetAvailabilityForDayAsync(
                barberId: 1,
                day: new DateTime(2025, 12, 21));

            int startIdx = 10 * 4; // 10:00
            Assert.False(availability[startIdx]);
            Assert.False(availability[startIdx + 1]);
        }

        [Fact]
        public async Task GetAvailabilityForDayAsync_UsesScheduleDay_WhenNoRequestExists()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = new ProfileService(db);

            var availability = await service.GetAvailabilityForDayAsync(
                barberId: 1,
                day: new DateTime(2025, 12, 7));

            Assert.NotNull(availability);
            Assert.True(availability.Cast<bool>().All(x => x == true));
        }

        [Fact]
        public async Task GetAvailabilityForDayAsync_ReturnsEmpty_WhenNoDataExists()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = new ProfileService(db);

            var availability = await service.GetAvailabilityForDayAsync(
                barberId: 1,
                day: new DateTime(2030, 1, 1));

            Assert.NotNull(availability);
            Assert.True(availability.Cast<bool>().All(x => x == false));
        }

        [Fact]
        public async Task SubmitReviewAsync_FirstReview_IsSavedAndNotified()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers.Include(b => b.Account).First();

            var service = new ProfileService(db);

            var dto = new SubmitReviewDto
            {
                UserId = 2,
                Barber = barber,
                ReviewModel = new ReviewModel
                {
                    Score = 5,
                    Text = "Excellent service",
                    OrderId = 0
                }
            };

            var result = await service.SubmitReviewAsync(dto);

            Assert.Equal(Severity.Success, result.Item2);
            Assert.Single(db.Reviews);

            var review = db.Reviews.First();
            Assert.Equal(5, review.Score);
            Assert.Equal(barber.Id, review.BarberId);

            Assert.Equal(1, barber.ReviewCount);
            Assert.Equal(5, barber.ReviewSum);

            Assert.Single(db.Notifications);
            Assert.Single(db.AccountNotifications);
        }

        [Fact]
        public async Task SubmitReviewAsync_DuplicateReview_ReturnsInfo()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers.First();

            db.Reviews.Add(new Review
            {
                BarberId = barber.Id,
                AccountId = 2,
                Score = 4,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            var service = new ProfileService(db);

            var dto = new SubmitReviewDto
            {
                UserId = 2,
                Barber = barber,
                ReviewModel = new ReviewModel
                {
                    Score = 5,
                    Text = "Trying again"
                }
            };

            var result = await service.SubmitReviewAsync(dto);

            Assert.Equal(Severity.Info, result.Item2);
            Assert.Single(db.Reviews); // still one
        }

        [Fact]
        public async Task SubmitReviewAsync_WhenReviewAlreadyExists_ReturnsInfoMessage()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers.First();

            // Existing review by the same user for the same barber
            db.Reviews.Add(new Review
            {
                BarberId = barber.Id,
                AccountId = 2,
                Score = 4,
                Text = "Existing review",
                CreatedAt = DateTime.Now
            });

            await db.SaveChangesAsync();

            var service = new ProfileService(db);

            var dto = new SubmitReviewDto
            {
                UserId = 2,              // same user
                Barber = barber,         // same barber
                ReviewModel = new ReviewModel
                {
                    Score = 5,
                    Text = "Trying to review again",
                    OrderId = 0
                }
            };

            // Act
            var result = await service.SubmitReviewAsync(dto);

            // Assert: correct branch result
            Assert.Equal(Severity.Info, result.Item2);
            Assert.Equal("Вы уже оставили отзыв об этом сотруднике", result.Item1);

            // Assert: no side effects
            Assert.Single(db.Reviews);              // no new review added
            Assert.Empty(db.Notifications);          // no notification sent
            Assert.Empty(db.AccountNotifications);   // no account notification
        }

    }
}
