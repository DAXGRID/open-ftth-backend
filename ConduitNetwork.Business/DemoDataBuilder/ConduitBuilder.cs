using Asset.Model;
using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.DemoDataBuilder
{
    public static class ConduitEventBuilder
    {
        /// <summary>
        /// Create MultiConduitPlaced event with detailed conduit information
        /// </summary>
        public static MultiConduitPlaced CreateMultiConduitPlacedEvent(Guid conduitId, Guid walkOfInteresId, string demoDataSpec)
        {
            string[] specSplit = demoDataSpec.Split('-');
            var conduitType = specSplit[0];
            var conduitNumber = specSplit[1];

            string conduitMarkingColor = null;

            if (specSplit.Length > 2)
                conduitMarkingColor = specSplit[2];

            int outerConduitDiameter = 0;
            int innerConduitDiameter = 0;

            // Default round shape
            var conduitShape = ConduitShapeKindEnum.Round;

            // Default orange color
            var conduitColor = ConduitColorEnum.Orange;

            // If flatliner
            if (conduitType.EndsWith("F"))
            {
                conduitShape = ConduitShapeKindEnum.Flat;
                conduitColor = ConduitColorEnum.Clear;
            }

            // Marking color
            var markingColor = ConduitColorEnum.None;

            if (conduitMarkingColor != null)
                markingColor = ColorCodeConverter.GetConduitColorFromCode(conduitMarkingColor);

            var assetInfo = new AssetInfo();

            // Handle common GM Plast conduit types
            if (conduitType.StartsWith("G"))
            {
                assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "GM Plast"
                };

                if (conduitType == "G12F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 12x12/8"
                    };
                }
                else if (conduitType == "G10F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 10x12/8"
                    };
                }
                else if (conduitType == "G6F")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Flatliner 6x12/8"
                    };
                }
                else
                    throw new ArgumentException("Don't know how to handle GM Plast conduit type: " + conduitType);
            }

            // Handle common Emetelle conduit types
            else if (conduitType.StartsWith("E"))
            {
                assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "Emetelle"
                };

                if (conduitType == "E10R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø50 10x12/8"
                    };

                    outerConduitDiameter = 50;
                }
                else if (conduitType == "E7R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø40 7x12/8"
                    };

                    outerConduitDiameter = 40;
                }
                else if (conduitType == "E5R")
                {
                    assetInfo.Model = new ProductModelInfo()
                    {
                        Name = "Ø35 5x12/8"
                    };

                    outerConduitDiameter = 35;
                }
                else
                    throw new ArgumentException("Don't know how to handle Emetelle conduit type: " + conduitType);
            }
            // Handle flex conduit
            else if (conduitType.StartsWith("FLEX"))
            {
                assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "GM Plast"
                };

                assetInfo.Model = new ProductModelInfo()
                {
                    Name = "Ø40 Flex"
                };

                outerConduitDiameter = 40;
                conduitColor = ConduitColorEnum.Red;
            }
            else
            {
                throw new Exception("Don't know how to handle conduit spec: " + conduitType);
            }

          

            var conduitInfo = new ConduitInfo()
            {
                Id = conduitId,
                Name = "R" + Numbers.GetNextConduitNumber(),
                Shape = conduitShape,
                Color = conduitColor,
                ColorMarking = markingColor,
                OuterDiameter = outerConduitDiameter,
                InnerDiameter = innerConduitDiameter
            };

            return new MultiConduitPlaced()
            {
                WalkOfInterestId = walkOfInteresId,
                MultiConduitId = conduitId,
                ConduitInfo = conduitInfo,
                AssetInfo = assetInfo
            };
        }

        public static SingleConduitPlaced CreateSingleConduitPlacedEvent(Guid conduitId, Guid walkOfInteresId, string demoDataSpec)
        {
            string[] specSplit = demoDataSpec.Split('-');
            var conduitType = specSplit[0];

            int outerConduitDiameter = 12;
            int innerConduitDiameter = 8;

            var conduitShape = ConduitShapeKindEnum.Round;

            // Default orange color
            var conduitColor = ConduitColorEnum.Orange;

            // Marking color
            var markingColor = ConduitColorEnum.None;

            var assetInfo = new AssetInfo();

            assetInfo.Manufacturer = new ManufacturerInfo()
                {
                    Name = "GM Plast"
                };
                assetInfo.Model = new ProductModelInfo()
                {
                        Name = "Ø12"
                };

            var conduitInfo = new ConduitInfo()
            {
                Id = conduitId,
                Name = "R" + Numbers.GetNextConduitNumber(),
                Shape = conduitShape,
                Color = conduitColor,
                ColorMarking = markingColor,
                OuterDiameter = outerConduitDiameter,
                InnerDiameter = innerConduitDiameter
            };

            return new SingleConduitPlaced()
            {
                WalkOfInterestId = walkOfInteresId,
                SingleConduitId = conduitId,
                ConduitInfo = conduitInfo,
                AssetInfo = assetInfo
            };
        }
        
        public static List<MultiConduitInnerConduitAdded> CreateInnerConduitAddedEvents(MultiConduitPlaced multiConduitPlacedEvent, string demoDataSpec)
        {
            List<MultiConduitInnerConduitAdded> result = new List<MultiConduitInnerConduitAdded>();

            string[] specSplit = demoDataSpec.Split('-');
            var conduitType = specSplit[0];

            int nInnerConduits = Convert.ToInt32(conduitType.Replace("G", "").Replace("E", "").Replace("R", "").Replace("F", ""));

            for (int i = 0; i < nInnerConduits; i++)
            {
                int parentIndex = i + 1;

                var conduitInfo = new ConduitInfo()
                {
                    Id = Guid.NewGuid(),
                    Name = "Subrør " + parentIndex,
                    Color = (ConduitColorEnum)parentIndex,
                    InnerDiameter = 8,
                    OuterDiameter = 12,
                    Shape = ConduitShapeKindEnum.Round,
                    ColorMarking = ConduitColorEnum.None
                };

                var innerConduitAddedEvent = new MultiConduitInnerConduitAdded()
                {
                    MultiConduitId = multiConduitPlacedEvent.ConduitInfo.Id,
                    MultiConduitIndex = parentIndex,
                    ConduitInfo = conduitInfo
                };

                result.Add(innerConduitAddedEvent);
            }

            return result;

        }
    }
}
