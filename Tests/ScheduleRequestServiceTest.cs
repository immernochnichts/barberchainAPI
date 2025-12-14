using barberchainAPI.Data;
using barberchainAPI.Functional.Services;
using Moq;
using Xunit;

namespace barberchainTest
{
    public class ScheduleRequestServiceTest
    {
        private static ScheduleRequestService CreateService(
            BarberchainDbContext db,
            Mock<INotificationService>? notificationMock = null)
        {
            notificationMock ??= new Mock<INotificationService>();

            var orderServiceMock = new Mock<IOrderService>();

            return new ScheduleRequestService(
                db,
                orderServiceMock.Object,
                notificationMock.Object
            );
        }


        [Fact]
        public async Task LoadRequestsAsync_ReturnsOnlyManagersRequests()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = CreateService(db);

            int managerId = 3;

            var result = await service.LoadRequestsAsync(managerId);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.All(result, r =>
                Assert.Equal(managerId, r.Barber.Bshop.ManagerAccountId));
        }

        [Fact]
        public async Task ApproveRequestAsync_UpdatesRequestStatus()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = CreateService(db);

            var req = db.ScheduleRequests.First(r => r.Status == ScheduleRequestStatus.Pending);

            await service.ApproveRequestAsync(req.Id);

            var updated = await db.ScheduleRequests.FindAsync(req.Id);

            Assert.Equal(ScheduleRequestStatus.Approved, updated!.Status);
        }

        [Fact]
        public async Task ApproveRequestAsync_CreatesScheduleDay_WhenNotExists()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = CreateService(db);

            var req = db.ScheduleRequests.First();

            var existing = db.BarberScheduleDays
                .FirstOrDefault(d => d.Date == req.RequestDate && d.BarberId == req.BarberId);

            Assert.Null(existing);

            await service.ApproveRequestAsync(req.Id);

            var created = db.BarberScheduleDays
                .FirstOrDefault(d => d.Date == req.RequestDate && d.BarberId == req.BarberId);

            Assert.NotNull(created);

            bool sequencesMatch = created!.AtuPattern
                .Cast<bool>()
                .Select((v, i) => v == req.AtuPattern[i])
                .All(x => x);

            Assert.True(sequencesMatch);
        }

        [Fact]
        public async Task ApproveRequestAsync_DeclinesOrders_WhenScheduleExists()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var service = CreateService(db);

            var req = db.ScheduleRequests
                .First(r => !string.IsNullOrEmpty(r.OrderIdsToDecline));

            // Create schedule beforehand
            db.BarberScheduleDays.Add(new BarberScheduleDay
            {
                BarberId = req.BarberId,
                Date = req.RequestDate,
                AtuPattern = req.AtuPattern
            });
            await db.SaveChangesAsync();

            await service.ApproveRequestAsync(req.Id);

            var declinedOrderIds = req.OrderIdsToDecline!
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse);

            var declinedOrders = db.Orders
                .Where(o => declinedOrderIds.Contains(o.Id))
                .ToList();

            Assert.All(declinedOrders, o =>
                Assert.Equal(OrderStatus.Declined, o.Status));
        }

        [Fact]
        public async Task NotifyBarberAsync_SendsNotificationToBarber()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var notificationMock = new Mock<INotificationService>();

            var service = CreateService(db, notificationMock);

            var req = db.ScheduleRequests.First();
            req.Status = ScheduleRequestStatus.Approved;
            await db.SaveChangesAsync();

            await service.NotifyBarberAsync(req);

            notificationMock.Verify(n =>
                n.NotifyAsync(
                    req.Barber.AccountId!.Value,
                    It.Is<Notification>(not =>
                        not.Content.Contains("одобрен"))),
                Times.Once);
        }

        [Fact]
        public async Task NotifyBarberAsync_SendsRejectionNotification()
        {
            var db = TestDbFactory.CreateInMemory();
            TestDbFactory.SeedData(db);

            var notificationMock = new Mock<INotificationService>();

            var service = CreateService(db, notificationMock);

            var req = db.ScheduleRequests.First();
            req.Status = ScheduleRequestStatus.Rejected;
            req.ReasonRejected = "Invalid schedule";
            await db.SaveChangesAsync();

            await service.NotifyBarberAsync(req);

            notificationMock.Verify(n =>
                n.NotifyAsync(
                    req.Barber.AccountId!.Value,
                    It.Is<Notification>(not =>
                        not.Content.Contains("отклонён"))),
                Times.Once);
        }

    }
}
