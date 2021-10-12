using Microsoft.Extensions.Logging;
using PnP.Core.Transformation.Model;
using PublishingMapping = PnP.Core.Transformation.SharePoint.MappingFiles.Publishing;
using PnP.Core.Transformation.SharePoint.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint.Services
{
    /// <summary>
    /// Specific layout transformator for the 'AutoDetect' layout option for publishing pages
    /// </summary>
    internal class PublishingLayoutTransformator
    {
        private ILogger<PublishingLayoutTransformator> logger;
        private readonly CorrelationService correlationService;
        private readonly IServiceProvider serviceProvider;


        public PublishingLayoutTransformator(ILogger<PublishingLayoutTransformator> logger,
            CorrelationService correlationService,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Builds the layout (sections) of the modern page
        /// </summary>
        /// <param name="pageLayout">The publishing page layout mapping to the current page</param>
        /// <param name="layout">Information about the source page</param>
        /// <param name="webPartsToProcess">Web Parts to process</param>
        public List<Section> Transform(PublishingMapping.PageLayout pageLayout, PageLayout layout, List<WebPartEntity> webPartsToProcess)
        {
            List<Column> GetColumns(int size)
            {
                return (from n in Enumerable.Range(0, size) select new Column()).ToList();
            }

            var result = new List<Section>();

            bool includeVerticalColumn = false;
            int verticalColumnEmphasis = 0;

            if (layout == Model.PageLayout.PublishingPage_AutoDetectWithVerticalColumn)
            {
                includeVerticalColumn = true;
                verticalColumnEmphasis = GetVerticalColumnBackgroundEmphasis(pageLayout);
            }

            // Should not occur, but to be on the safe side...
            if (webPartsToProcess.Count == 0)
            {
                result.Add(
                    new Section
                    {
                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                        Columns = GetColumns(1),
                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, 1),
                    });

                return result;
            }

            var firstRow = webPartsToProcess.OrderBy(p => p.Row).First().Row;
            var lastRow = webPartsToProcess.OrderBy(p => p.Row).Last().Row;

            // Loop over the possible rows...will take in account possible row gaps
            // Each row means a new section
            int sectionOrder = 1;
            for (int rowIterator = firstRow; rowIterator <= lastRow; rowIterator++)
            {
                var webpartsInRow = webPartsToProcess.Where(p => p.Row == rowIterator);
                if (webpartsInRow.Any())
                {
                    // Determine max column number
                    int maxColumns = 1;

                    foreach (var wpInRow in webpartsInRow)
                    {
                        if (wpInRow.Column > maxColumns)
                        {
                            maxColumns = wpInRow.Column;
                        }
                    }

                    // Deduct the vertical column 
                    if (includeVerticalColumn && rowIterator == firstRow)
                    {
                        maxColumns--;
                    }

                    if (maxColumns > 3)
                    {
                        this.logger.LogError(
                            SharePointTransformationResources.Error_Maximum3ColumnsAllowed);
                        throw new Exception(SharePointTransformationResources.Error_Maximum3ColumnsAllowed);
                    }
                    else
                    {
                        if (maxColumns == 1)
                        {
                            if (includeVerticalColumn && rowIterator == firstRow)
                            {
                                result.Add(
                                    new Section
                                    {
                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumnVerticalSection,
                                        Order = sectionOrder,
                                        Columns = GetColumns(1),
                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                    });
                            }
                            else
                            {
                                result.Add(
                                    new Section
                                    {
                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                                        Order = sectionOrder,
                                        Columns = GetColumns(1),
                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                    });
                            }
                        }
                        else if (maxColumns == 2)
                        {
                            // if we've only an image in one of the columns then make that one the 'small' column
                            var imageWebPartsInRow = webpartsInRow.Where(p => p.Type == WebParts.WikiImage);
                            if (imageWebPartsInRow.Any())
                            {
                                Dictionary<int, int> imageWebPartsPerColumn = new Dictionary<int, int>();
                                foreach (var imageWebPart in imageWebPartsInRow.OrderBy(p => p.Column))
                                {
                                    if (imageWebPartsPerColumn.TryGetValue(imageWebPart.Column, out int wpCount))
                                    {
                                        imageWebPartsPerColumn[imageWebPart.Column] = wpCount + 1;
                                    }
                                    else
                                    {
                                        imageWebPartsPerColumn.Add(imageWebPart.Column, 1);
                                    }
                                }

                                var firstImageColumn = imageWebPartsPerColumn.First();
                                var secondImageColumn = imageWebPartsPerColumn.Last();

                                if (firstImageColumn.Key == secondImageColumn.Key)
                                {
                                    // there was only one column with images
                                    var firstImageColumnOtherWebParts = webpartsInRow.Where(p => p.Column == firstImageColumn.Key && p.Type != WebParts.WikiImage);
                                    if (!firstImageColumnOtherWebParts.Any())
                                    {
                                        // no other web parts in this column
                                        var orderedList = webpartsInRow.OrderBy(p => p.Column).First();

                                        if (orderedList.Column == firstImageColumn.Key)
                                        {
                                            // image left
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnRightVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnRight,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                        else
                                        {
                                            // image right
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeftVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeft,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (includeVerticalColumn && rowIterator == firstRow)
                                        {
                                            result.Add(
                                                new Section
                                                {
                                                    CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnVerticalSection,
                                                    Order = sectionOrder,
                                                    Columns = GetColumns(2),
                                                    ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                });
                                        }
                                        else
                                        {
                                            result.Add(
                                                new Section
                                                {
                                                    CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                                                    Order = sectionOrder,
                                                    Columns = GetColumns(2),
                                                    ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                });
                                        }
                                    }
                                }
                                else
                                {
                                    if (firstImageColumn.Value == 1 || secondImageColumn.Value == 1)
                                    {
                                        // does one of the two columns have anything else besides image web parts
                                        var firstImageColumnOtherWebParts = webpartsInRow.Where(p => p.Column == firstImageColumn.Key && p.Type != WebParts.WikiImage);
                                        var secondImageColumnOtherWebParts = webpartsInRow.Where(p => p.Column == secondImageColumn.Key && p.Type != WebParts.WikiImage);

                                        if (!firstImageColumnOtherWebParts.Any() && !secondImageColumnOtherWebParts.Any())
                                        {
                                            // two columns with each only one image...
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                        else if (!firstImageColumnOtherWebParts.Any() && secondImageColumnOtherWebParts.Any())
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnRightVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnRight,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                        else if (firstImageColumnOtherWebParts.Any() && !secondImageColumnOtherWebParts.Any())
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeftVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeft,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                        else
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnVerticalSection,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                    });
                                            }
                                            else
                                            {
                                                result.Add(
                                                    new Section
                                                    {
                                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                                                        Order = sectionOrder,
                                                        Columns = GetColumns(2),
                                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (includeVerticalColumn && rowIterator == firstRow)
                                        {
                                            result.Add(
                                                new Section
                                                {
                                                    CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnVerticalSection,
                                                    Order = sectionOrder,
                                                    Columns = GetColumns(2),
                                                    ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                    VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                                });
                                        }
                                        else
                                        {
                                            result.Add(
                                                new Section
                                                {
                                                    CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                                                    Order = sectionOrder,
                                                    Columns = GetColumns(2),
                                                    ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                                });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (includeVerticalColumn && rowIterator == firstRow)
                                {
                                    result.Add(
                                        new Section
                                        {
                                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnVerticalSection,
                                            Order = sectionOrder,
                                            Columns = GetColumns(2),
                                            ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                            VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                        });
                                }
                                else
                                {
                                    result.Add(
                                        new Section
                                        {
                                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                                            Order = sectionOrder,
                                            Columns = GetColumns(2),
                                            ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                        });
                                }
                            }
                        }
                        else if (maxColumns == 3)
                        {
                            if (includeVerticalColumn && rowIterator == firstRow)
                            {
                                result.Add(
                                    new Section
                                    {
                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumnVerticalSection,
                                        Order = sectionOrder,
                                        Columns = GetColumns(3),
                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                        VerticalSectionZoneEmphasis = verticalColumnEmphasis,
                                    });
                            }
                            else
                            {
                                result.Add(
                                    new Section
                                    {
                                        CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumn,
                                        Order = sectionOrder,
                                        Columns = GetColumns(3),
                                        ZoneEmphasis = GetBackgroundEmphasis(pageLayout, rowIterator),
                                    });
                            }
                        }

                        sectionOrder++;
                    }
                }
                else
                {
                    // non used row...ignore
                }
            }

            return result;
        }

        #region Helper methods
        private int GetBackgroundEmphasis(PublishingMapping.PageLayout pageLayout, int row)
        {
            PublishingMapping.BackgroundEmphasis emphasis = PublishingMapping.BackgroundEmphasis.None;

            if (pageLayout != null)
            {
                if (pageLayout.SectionEmphasis != null && pageLayout.SectionEmphasis.Section != null)
                {
                    var section = pageLayout.SectionEmphasis.Section.Where(p => p.Row == row).FirstOrDefault();
                    if (section != null)
                    {
                        return BackgroundEmphasisToInt(section.Emphasis);
                    }
                }
            }

            return BackgroundEmphasisToInt(emphasis);
        }

        private int GetVerticalColumnBackgroundEmphasis(PublishingMapping.PageLayout pageLayout)
        {
            PublishingMapping.BackgroundEmphasis emphasis = PublishingMapping.BackgroundEmphasis.None;

            if (pageLayout != null)
            {
                if (pageLayout.SectionEmphasis != null && pageLayout.SectionEmphasis.VerticalColumnEmphasisSpecified)
                {
                    return BackgroundEmphasisToInt(pageLayout.SectionEmphasis.VerticalColumnEmphasis);
                }
            }

            return BackgroundEmphasisToInt(emphasis);
        }

        private int BackgroundEmphasisToInt(PublishingMapping.BackgroundEmphasis emphasis)
        {
            switch (emphasis)
            {
                case PublishingMapping.BackgroundEmphasis.None: return 0;
                case PublishingMapping.BackgroundEmphasis.Neutral: return 1;
                case PublishingMapping.BackgroundEmphasis.Soft: return 2;
                case PublishingMapping.BackgroundEmphasis.Strong: return 3;
            }

            return 0;
        }
        #endregion
    }
}
