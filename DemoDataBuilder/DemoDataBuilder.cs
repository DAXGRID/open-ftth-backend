using DemoDataBuilder.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DemoDataBuilder
{
    /// <summary>
    /// Builds a detailed fiber demo network database, by reading from a simple database holding a manually created route network with build codes.
    /// In the simple database, an attribut called BuildDemoData is filled out on all nodes and segments. The attribut contains a semicolon seperated list 
    /// of codes telling the builder what types of conduits and equipments it should place inside the specific route element.
    /// </summary>
    public class DemoDataBuilder
    {
        MigrationBuilder _migrationBuilder;

        public DemoDataBuilder(MigrationBuilder migrationBuilder)
        {
            _migrationBuilder = migrationBuilder;
        }

        public void Run()
        {
            RouteNetworkBuilder.Run(_migrationBuilder);

            ConduitBuilder.Run(_migrationBuilder);
        }
    }
}
