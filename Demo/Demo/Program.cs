using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Z.EntityFramework.Plus;

namespace Demo
{
    class Program
    {
 
        static void Main(string[] args)
        {
            var tenent1 = Guid.NewGuid();
            var tenent2 = Guid.NewGuid();
            int customerId = 1;

            Seed(tenent1, customerId, "Derek Comartin");
            Seed(tenent2, customerId, "CodeOpinion.com");

            using (var db = DbFactory(tenent1))
            {
                var customer = db.Customers.Single(x => x.CustomerId == customerId);
                Console.WriteLine($"Hello {customer.Name}");
            }

            using (var db = DbFactory(tenent2))
            {
                var customer = db.Customers.Single(x => x.CustomerId == customerId);
                Console.WriteLine($"Hello {customer.Name}");
            }

            Console.ReadKey();
        }

        private static MyDbContext DbFactory(Guid tenentId)
        {
            var connection = @"Server=(localdb)\mssqllocaldb;Database=EFCoreMultiTenent;Trusted_Connection=True;";
            return new MyDbContext(tenentId, connection);
        }

        static void Seed(Guid tenentId, int customerId, string name)
        {
            using (var db = DbFactory(tenentId))
            {
                db.Customers.Add(new Customer
                {
                    CustomerId = customerId,
                    TenentId = tenentId,
                    Name = name
                });
                db.SaveChanges();
            }
        }
    }

    public class Customer
    {
        public Guid TenentId { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public Guid TenentId { get; }
        public string Connection { get; }
        public DbSet<Customer> Customers { get; set; }

        public MyDbContext(Guid tenentId, string connection)
        {
            TenentId = tenentId;
            Connection = connection;

            this.Filter<Customer>(x => x.Where(q => q.TenentId == TenentId));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Connection);
        }
    }

    
}
