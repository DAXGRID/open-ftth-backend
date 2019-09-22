using Microsoft.EntityFrameworkCore.Migrations;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoDataBuilder.Builders
{
    public static class SpecificationBuilder
    {
        public static Guid[] FlatlinerInnerConduitsColorMarkingSpecificationIds;

        public static void Run(MigrationBuilder migrationBuilder)
        {
            // Create color specifications used in GM Plast flatliners
            FlatlinerInnerConduitsColorMarkingSpecificationIds = CreateColorMarkingSpecifications(migrationBuilder);

            // Create GM Plast flatliner 12x12/8 specification
            var flatSpecId = new Guid("621023aa-4bff-4819-a3b9-6a494b4319f7");

            migrationBuilder.InsertData(
               table: "ConduitSpecifications",
               columns: new[] { "Id", "ParentSpecId", "ConduitKind", "ConduitShapeKind", "SeqNo", "OuterDiameter", "InnerDiameter", "Manufacturer", "ProductModel", "ColorSpecificationId" },
               values: new object[] {
                   new Guid("621023aa-4bff-4819-a3b9-6a494b4319f7"),
                   Guid.Empty,
                   (int)ConduitKindEnum.MultiConduit,
                   (int)ConduitShapeKindEnum.Flat,
                   0,
                   0,
                   0,
                   "GM PLAST",
                   "Flatliner 8x12/8",
                   new Guid("706eb29e-4675-4d3e-b67f-86bb009f5bc2")
               });

            var flatInner1Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 1, FlatlinerInnerConduitsColorMarkingSpecificationIds[0]);
            var flatInner2Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 2, FlatlinerInnerConduitsColorMarkingSpecificationIds[1]);
            var flatInner3Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 3, FlatlinerInnerConduitsColorMarkingSpecificationIds[2]);
            var flatInner4Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 4, FlatlinerInnerConduitsColorMarkingSpecificationIds[3]);
            var flatInner5Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 5, FlatlinerInnerConduitsColorMarkingSpecificationIds[4]);
            var flatInner6Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 6, FlatlinerInnerConduitsColorMarkingSpecificationIds[5]);
            var flatInner7Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 7, FlatlinerInnerConduitsColorMarkingSpecificationIds[6]);
            var flatInner8Id = CreateFlatlinerInnerDuctSpec(migrationBuilder, flatSpecId, 8, FlatlinerInnerConduitsColorMarkingSpecificationIds[7]);
        }

        public static Guid CreateFlatlinerInnerDuctSpec(MigrationBuilder migrationBuilder, Guid parentSpecId, int seqNo, Guid colorMarkingSpecId)
        {
            Guid id = Guid.NewGuid();

            migrationBuilder.InsertData(
              table: "ConduitSpecifications",
              columns: new[] { "Id", "ParentSpecId", "ConduitKind", "ConduitShapeKind", "SeqNo", "OuterDiameter", "InnerDiameter", "Manufacturer", "ProductModel", "ColorSpecificationId" },
              values: new object[] {
                   id,
                   parentSpecId,
                   (int)ConduitKindEnum.InnerConduit,
                   (int)ConduitShapeKindEnum.Round,
                   seqNo,
                   12,
                   8,
                   "GM PLAST",
                   "Flatliner 12/8 Inner Duct",
                   colorMarkingSpecId
              });

            return id;
        }

        public static Guid[] CreateColorMarkingSpecifications(MigrationBuilder migrationBuilder)
        {
            Guid[] result = new Guid[8];

            for (int i = 0; i < 8; i++)
                result[i] = Guid.NewGuid();

            // Blue
            migrationBuilder.InsertData(
              table: "ColorMarkingSpecifications",
              columns: new[] { "Id", "Name", "Alias", "Marking", "ColorCode" },
              values: new object[] {
                   result[0],
                   "RD",
                   "Rød",
                   null,
                   "#0000FF"
              });

            // Yellow
            migrationBuilder.InsertData(
              table: "ColorMarkingSpecifications",
              columns: new[] { "Id", "Name", "Alias", "Marking", "ColorCode" },
              values: new object[] {
                   result[1],
                   "YL",
                   "Gul",
                   null,
                   "#FFFF00"
              });


            return result;
        }
    }
}
