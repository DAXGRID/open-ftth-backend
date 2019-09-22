using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Specifications
{
    public static class ConduitSpecificationMockupBuilder
    {
        public static Dictionary<Guid, ConduitSpecification> CreateSpecifications()
        {
            Dictionary<Guid,ConduitSpecification> result = new Dictionary<Guid, ConduitSpecification>();

            List<string> codes = new List<string>() { "G12F", "G10F", "G6F", "E10R", "E7R", "E5R" };

            foreach (var code in codes)
            {
                var spec = CreateMultiConduitSpecification(code);
                result.Add(spec.Id, spec);
            }

            // Create 12 mm single conduits
            for (int i = 1; i <= 12; i++)
            {
                int outerDiameter = 12;
                int innerDiameter = 8;
                var color = (ConduitColorEnum)i;
                var spec = CreateSingleConduitSpecification("Unknown", "Ø12 " + color, color, outerDiameter, innerDiameter);
                result.Add(spec.Id, spec);
            }

            // Create 16 mm single conduits
            for (int i = 1; i <= 12; i++)
            {
                int outerDiameter = 16;
                int innerDiameter = 10;
                var color = (ConduitColorEnum)i;
                var spec = CreateSingleConduitSpecification("Unknown", "Ø16 " + color, color, outerDiameter, innerDiameter);
                result.Add(spec.Id, spec);
            }


            return result;
        }

        private static ConduitSpecification CreateMultiConduitSpecification(string code)
        {
            int outerConduitOuterDiameter = 0;
            int outerConduitInnerDiameter = 0;

            int innerConduitOuterDiameter = 0;
            int innerConduitInnerDiameter = 0;

            // Default round shape
            var conduitShape = ConduitShapeKindEnum.Round;

            // Default orange color
            var conduitColor = ConduitColorEnum.Orange;

            // If flatliner
            if (code.EndsWith("F"))
            {
                conduitShape = ConduitShapeKindEnum.Flat;
                conduitColor = ConduitColorEnum.Clear;
            }

            var numberOfInnerConduits = 0;

           
            var assetInfo = new AssetInfo();

            // Handle common GM Plast conduit types
            if (code.StartsWith("G"))
            {
                assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "GM Plast"
                };

                if (code == "G12F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 12x12/8"
                    };

                    numberOfInnerConduits = 12;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;
                }
                else if (code == "G10F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 10x12/8"
                    };

                    numberOfInnerConduits = 10;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;
                }
                else if (code == "G6F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 6x12/8"
                    };

                    numberOfInnerConduits = 6;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;
                }
                else
                    throw new ArgumentException("Don't know how to handle GM Plast conduit type: " + code);
            }

            // Handle common Emetelle conduit types
            if (code.StartsWith("E"))
            {
                assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "Emetelle"
                };

                if (code == "E10R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø50 10x12/8"
                    };
                    outerConduitOuterDiameter = 50;

                    numberOfInnerConduits = 10;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;

                }
                else if (code == "E7R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø40 7x12/8"
                    };

                    outerConduitOuterDiameter = 40;

                    numberOfInnerConduits = 7;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;

                }
                else if (code == "E5R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø35 5x12/8"
                    };

                    outerConduitOuterDiameter = 35;

                    numberOfInnerConduits = 5;
                    innerConduitOuterDiameter = 12;
                    innerConduitInnerDiameter = 8;

                }
                else
                    throw new ArgumentException("Don't know how to handle Emetelle conduit type: " + code);
            }

            var conduitSpec = new ConduitSpecification()
            {
                Id = GUIDHelper.StringToGUID(code),
                Kind = ConduitKindEnum.MultiConduit,
                Shape = conduitShape,
                Color = conduitColor,
                OuterDiameter = outerConduitOuterDiameter,
                InnerDiameter = outerConduitInnerDiameter,
                ProductModels = new List<ProductModelInfo>() {
                     new ProductModelInfo()
                    {
                        Id = GUIDHelper.StringToGUID(assetInfo.Manufacturer.Name + ":" + assetInfo.Model.Name),
                        Name = assetInfo.Model.Name,
                        Manufacturer = new ManufacturerInfo()
                        {
                            Id = GUIDHelper.StringToGUID(assetInfo.Manufacturer.Name),
                            Name = assetInfo.Manufacturer.Name
                        }
                    }
                }
            };


            // Create the children
            conduitSpec.ChildSpecifications = new List<ConduitSpecification>();

            for (int i = 1; i <= numberOfInnerConduits;i++)
            {
                conduitSpec.ChildSpecifications.Add(CreateInnerConduitSpecification(code, i, innerConduitOuterDiameter, innerConduitInnerDiameter));
            }

            return conduitSpec;
        }

        private static ConduitSpecification CreateInnerConduitSpecification(string multiConduitCode, int innerConduitNumber, int outerDiameter, int innerDiameter)
        {
            var conduitSpec = new ConduitSpecification()
            {
                Id = GUIDHelper.StringToGUID(multiConduitCode + ":" + innerConduitNumber),
                SequenceNumber = innerConduitNumber,
                Kind = ConduitKindEnum.InnerConduit,
                Shape = ConduitShapeKindEnum.Round,
                Color = (ConduitColorEnum)innerConduitNumber,
                OuterDiameter = outerDiameter,
                InnerDiameter = innerDiameter
            };

            return conduitSpec;
        }

        private static ConduitSpecification CreateSingleConduitSpecification(string manufacturer, string productModel, ConduitColorEnum color, int outerDiameter, int innerDiameter)
        {
            var conduitSpec = new ConduitSpecification()
            {
                Id = GUIDHelper.StringToGUID(color + ":" + outerDiameter + ":" + innerDiameter),
                Kind = ConduitKindEnum.SingleConduit,
                Shape = ConduitShapeKindEnum.Round,
                Color = color,
                OuterDiameter = outerDiameter,
                InnerDiameter = innerDiameter,
                ProductModels = new List<ProductModelInfo>()
                {
                    new ProductModelInfo()
                    {
                        Id = GUIDHelper.StringToGUID(manufacturer + ":" + productModel),
                        Name = productModel,
                        Manufacturer = new ManufacturerInfo()
                        {
                            Id = GUIDHelper.StringToGUID(manufacturer),
                            Name = manufacturer
                        }
                    }
                }
            };

            return conduitSpec;
        }

    }
}
