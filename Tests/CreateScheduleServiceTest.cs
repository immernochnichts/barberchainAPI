using barberchainAPI.Data;
using barberchainAPI.Functional.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace barberchainTest
{
    public class CreateScheduleServiceTest
    {
        [Fact]
        public async Task CreateScheduleRequest_ExistingScheduleLoadedOnClick()
        {
            var db = TestDbFactory.CreateInMemory();

            TestDbFactory.SeedData(db);

            var service = new CreateScheduleService(db);
            
            var result = await service.LoadBarberScheduleAsync(1, new DateOnly(2025, 12, 7));

            Assert.NotNull(result);
            Assert.True(result.AtuPattern.Cast<bool>().All(x => x == true));
        }

        [Fact]
        public async Task CreateScheduleRequest_DefaultScheduleLoadedOnClick()
        {
            var db = TestDbFactory.CreateInMemory();

            TestDbFactory.SeedData(db);

            var service = new CreateScheduleService(db);

            var result = await service.LoadBarberScheduleAsync(1, new DateOnly(1, 1, 1));
            var defaultSched = (await db.Barbershops.FindAsync(1))!.DefaultSchedule;

            Assert.NotNull(result);
            bool sequencesMatch = result.AtuPattern
                .Cast<bool>()
                .Select((v, i) => v == defaultSched[i])
                .All(equal => equal);

            Assert.True(sequencesMatch);
        }

        [Fact]
        public async Task CreateScheduleRequest_NewScheduleDayCreatedOnSubmitIfNotExists()
        {
            var db = TestDbFactory.CreateInMemory();

            TestDbFactory.SeedData(db);

            var service = new CreateScheduleService(db);

            BitArray newSched = new BitArray(Enumerable.Repeat<bool>(false, 96).ToArray());
            newSched[2] = true;

            CreateReplaceSchedDto dto = new CreateReplaceSchedDto()
            {
                AtuPattern = newSched,
                Barber = db.Barbers.Where(b => b.Id == 1).First(),
                Message = "NewScheduleDayCreatedOnSubmitIsNotExistsTest",
                OrdersToDecline = new List<Order>(),
                SelectedDate = new DateOnly(1, 1, 1) // non-existent in bsd table
            };

            var result = await service.CreateOrReplaceScheduleRequest(dto);

            Assert.NotNull(result);

            var newReq = db.ScheduleRequests.Where(r => r.RequestDate == result.Request.RequestDate).First();

            Assert.NotNull(newReq);
            bool sequencesMatch = result.Request.AtuPattern
                .Cast<bool>()
                .Select((v, i) => v == newReq.AtuPattern[i])
                .All(equal => equal);
            Assert.True(sequencesMatch);
            Assert.True(result.Request.Status == ScheduleRequestStatus.Pending);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task CreateScheduleRequest_ReplacesExistingRequest()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers.First();

            var existingRequest = new ScheduleRequest
            {
                BarberId = barber.Id,
                RequestDate = new DateOnly(2025, 12, 10),
                AtuPattern = new BitArray(Enumerable.Repeat(false, 96).ToArray()),
                Status = ScheduleRequestStatus.Approved,
                Message = "Old"
            };

            db.ScheduleRequests.Add(existingRequest);
            db.SaveChanges();

            var service = new CreateScheduleService(db);

            var newPattern = new BitArray(Enumerable.Repeat(true, 96).ToArray());

            var dto = new CreateReplaceSchedDto
            {
                Barber = barber,
                SelectedDate = existingRequest.RequestDate,
                AtuPattern = newPattern,
                Message = "Updated",
                OrdersToDecline = new()
            };

            var result = await service.CreateOrReplaceScheduleRequest(dto);

            Assert.NotNull(result.Request);
            Assert.Null(result.Error);
            Assert.Equal(existingRequest.Id, result.Request.Id);
            Assert.Equal("Updated", result.Request.Message);
            Assert.Equal(ScheduleRequestStatus.Pending, result.Request.Status);
        }

        [Fact]
        public void GetScheduleChanges_ReturnsCorrectDiff()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers
                .Include(b => b.Bshop)
                .First();

            var pattern = new BitArray(barber.Bshop.DefaultSchedule);

            for (int i = 36; i <= 39; i++)
                pattern[i] = false;

            var request = new ScheduleRequest
            {
                BarberId = barber.Id,
                RequestDate = new DateOnly(2025, 12, 11),
                AtuPattern = pattern,
                Status = ScheduleRequestStatus.Pending,
                Message = "REQUEST MESSAGE"
            };

            db.ScheduleRequests.Add(request);
            db.SaveChanges();

            var service = new CreateScheduleService(db);

            var changes = service.GetScheduleChanges(request.Id);

            Assert.Contains("09:00–10:00 סעאכמ םוהמסעףןםמ", changes);
        }

        [Fact]
        public async Task NotifyManagerAsync_CreatesNotificationAndLinksManager()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            // Add manager
            var manager = new Account
            {
                Id = 10,
                Email = "manager@test.com",
                Lastname = "Manager",
                Hash = new byte[60]
            };

            db.Accounts.Add(manager);

            var shop = db.Barbershops.First();
            shop.ManagerAccountId = manager.Id;

            db.SaveChanges();

            var barber = db.Barbers
                .Include(b => b.Account)
                .Include(b => b.Bshop)
                .First();

            var service = new CreateScheduleService(db);

            await service.NotifyManagerAsync(barber, new DateTime(2025, 12, 12));

            var notification = db.Notifications.FirstOrDefault();
            var accountNotification = db.AccountNotifications.FirstOrDefault();

            Assert.NotNull(notification);
            Assert.NotNull(accountNotification);
            Assert.Equal(manager.Id, accountNotification.AccountId);
            Assert.Equal(notification.Id, accountNotification.NotificationId);
        }

        [Fact]
        public async Task CancelChangesAsync_ResetsPatternAndOccupiedIndexes()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var barber = db.Barbers
                .Include(b => b.Bshop)
                .First();

            var order = new Order
            {
                BarberId = barber.Id,
                AppointedTime = new DateTime(2025, 12, 7, 10, 0, 0),
                Status = OrderStatus.Pending,
                OrderJobs = new List<OrderJob>
                {
                    new OrderJob
                    {
                        Job = new Job { Name = "ׁענטזךא", Descr = "אמאמאממאמאמאמא", DurationAtu = 4 }
                    }
                }
            };

            db.Orders.Add(order);
            db.SaveChanges();

            var dto = new CancelChangesDto
            {
                Barber = barber,
                SelectedDate = new DateOnly(2025, 12, 7),
                AtuPattern = new BitArray(96),
                Orders = new List<Order> { order },
                OrdersToDecline = new List<Order> { order },
                OccupiedIndexes = new List<int>()
            };

            var service = new CreateScheduleService(db);

            await service.CancelChangesAsync(dto);

            Assert.Empty(dto.OrdersToDecline);
            Assert.True(dto.OccupiedIndexes.Count > 0);
        }
    }
}