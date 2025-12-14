using Microsoft.EntityFrameworkCore;
using barberchainAPI.Data;
using System.Collections;

namespace barberchainTest
{
    public static class TestDbFactory
    {
        public static BarberchainDbContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<BarberchainDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh db per test
                .EnableSensitiveDataLogging()
                .Options;

            var db = new BarberchainDbContext(options);

            db.Database.EnsureCreated();

            return db;
        }

        public static void SeedData(BarberchainDbContext db)
        {
            var account = new Account
            {
                Id = 1,
                Email = "iambarber@gmail.com",
                Lastname = "TestBarber",
                Hash = Enumerable.Repeat<byte>(0, 60).ToArray()
            };

            var account2 = new Account
            {
                Id = 2,
                Email = "iamuser@gmail.com",
                Lastname = "TestUser",
                Hash = Enumerable.Repeat<byte>(42, 60).ToArray()
            };

            var account3 = new Account
            {
                Id = 3,
                Email = "iammanager@gmail.com",
                Lastname = "TestManager",
                Hash = Enumerable.Repeat<byte>(23, 60).ToArray()
            };

            var shop = new Barbershop
            {
                Id = 1,
                Phone = "+79555553542",
                Latitude = 41,
                Longitude = 41,
                Address = "ул. НеЛенина 1",
                DefaultSchedule = new BitArray(Enumerable.Repeat(true, 96).ToArray()),
                ManagerAccountId = 3
            };

            var barber = new Barber
            {
                Id = 1,
                Account = account,
                AccountId = account.Id,
                Bshop = shop,
                BshopId = shop.Id
            };

            var bsd = new BarberScheduleDay
            {
                BarberId = barber.Id,
                Barber = barber,
                Date = new DateOnly(2025, 12, 7),
                AtuPattern = new BitArray(Enumerable.Repeat(true, 96).ToArray())
            };

            Job job1 = new Job()
            {
                Id = 1,
                ColorCSS = "black",
                DefaultPrice = 800,
                Descr = "DESCRIPTION MAAFUCKKAAAAA",
                DurationAtu = 4,
                Name = "HAIRCUT MAAFUCKAAAA"
            };

            Order order = new Order()
            {
                Id = 1,
                Method = OrderMethod.Online,
                OrderTime = new DateTime(2025, 12, 9, 14, 15, 15, DateTimeKind.Local),
                AccountId = 2,
                Account = account2,
                IsPaid = true,
                OrderJobs = new List<OrderJob>()
                {
                    new OrderJob() { JobId = 1, OrderId = 1 }
                },
                AppointedTime = new DateTime(2025, 12, 10, 16, 0, 0, DateTimeKind.Local),
                Phone = null,
                Status = OrderStatus.Waiting
            };

            var req1 = new ScheduleRequest()
            {
                AtuPattern = new BitArray(Enumerable.Repeat(false, 96).ToArray()),
                RequestDate = new DateOnly(2025, 12, 21),
                BarberId = 1,
                CreatedAt = new DateTime(2025, 12, 10, 13, 15, 33, DateTimeKind.Local),
                Status = ScheduleRequestStatus.Pending,
                Message = "TEST MESSAGE MAAFUCKAA",
                OrderIdsToDecline = ""
            };

            var req2 = new ScheduleRequest()
            {
                AtuPattern = new BitArray(Enumerable.Repeat(false, 96).ToArray()),
                RequestDate = new DateOnly(2025, 12, 23),
                BarberId = 1,
                CreatedAt = new DateTime(2025, 12, 10, 13, 16, 14, DateTimeKind.Local),
                Status = ScheduleRequestStatus.Pending,
                Message = "TEST MESSAGE MAAFUCKAA",
                OrderIdsToDecline = "1"
            };

            db.Accounts.Add(account);
            db.Barbershops.Add(shop);
            db.Barbers.Add(barber);
            db.BarberScheduleDays.Add(bsd);
            db.Jobs.Add(job1);
            db.Orders.Add(order);
            db.ScheduleRequests.Add(req1);
            db.ScheduleRequests.Add(req2);

            db.SaveChanges();
        }
    }

}
