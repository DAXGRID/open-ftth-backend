using EquipmentService.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Linq;

namespace DemoDataBuilder.Tests
{
    [TestClass]
    public class UnitTest1
    {
        /*
        [TestMethod]
        public void RunMigration()
        {
            var sqlConString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

            var options = new DbContextOptionsBuilder<EquipmentServiceDbContext>()
                .UseNpgsql(sqlConString)
                .Options;

            // Run the test against one instance of the context
            using (var context = new EquipmentServiceDbContext(options))
            {
                context.Database.Migrate();
            }
        }
        */
    }
}
