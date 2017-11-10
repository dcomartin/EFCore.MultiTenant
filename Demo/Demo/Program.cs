using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Demo
{
    class Program
    {
 
        static void Main(string[] args)
        {
            var tenant1 = Guid.NewGuid();
            var tenant2 = Guid.NewGuid();
            int customerId = 1;

            Seed(tenant1, customerId, "Derek Comartin");
            Seed(tenant2, customerId, "CodeOpinion.com");

            using (var db = DbFactory(tenant1))
            {
                var customer = db.Customers.Single(x => x.CustomerId == customerId);
                Console.WriteLine($"Hello {customer.Name}");
            }

            using (var db = DbFactory(tenant2))
            {
                var customer = db.Customers.Single(x => x.CustomerId == customerId);
                Console.WriteLine($"Hello {customer.Name}");
            }

            Console.ReadKey();
        }

        private static MyDbContext DbFactory(Guid tenantId)
        {
            var connection = @"Server=(localdb)\mssqllocaldb;Database=EFCoreMultiTenant;Trusted_Connection=True;";
            return new MyDbContext(tenantId, connection);
        }

        static void Seed(Guid tenantId, int customerId, string name)
        {
            using (var db = DbFactory(tenantId))
            {
                db.Customers.Add(new Customer
                {
                    CustomerId = customerId,
                    TenantId = tenantId,
                    Name = name
                });
                db.SaveChanges();
            }
        }
    }

    public class Customer
    {
        public Guid TenantId { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public Guid TenantId { get; }
        public string Connection { get; }
        public DbSet<Customer> Customers { get; set; }

        public MyDbContext(Guid tenantId, string connection)
        {
            TenantId = tenantId;
            Connection = connection;

            this.Filter<Customer>(x => x.Where(q => q.TenantId == TenantId));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Connection);
        }
    }

    
}
