using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PnP.Core.Modernization.Publishing
{
    /// <summary>
    /// Specific layout transformator for the 'AutoDetect' layout option for publishing pages
    /// </summary>
    public class PublishingLayoutTransformator : BaseTransform, ILayoutTransformator
    {
        private IPage page;
        private PageLayout pageLayoutMappingModel;

        #region Construction
        /// <summary>
        /// Creates a layout transformator instance
        /// </summary>
        /// <param name="page">Client side page that will be receive the created layout</param>
        /// <param name="pageLayoutMappingModel"></param>
        /// <param name="logObservers"></param>
        public PublishingLayoutTransformator(IPage page, PageLayout pageLayoutMappingModel, IList<ILogObserver> logObservers = null)
        {
            // Register observers
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            this.page = page;
            this.pageLayoutMappingModel = pageLayoutMappingModel;
        }
        #endregion

        /// <summary>
        /// Builds the layout (sections) of the modern page
        /// </summary>
        /// <param name="pageData">Information about the source page</param>
        public void Transform(Tuple<Pages.PageLayout, List<WebPartEntity>> pageData)
        {

            bool includeVerticalColumn = false;
            int verticalColumnEmphasis = 0;

            if (pageData.Item1 == Pages.PageLayout.PublishingPage_AutoDetectWithVerticalColumn || pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnRightVerticalSection || pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnLeftVerticalSection)
            {
                includeVerticalColumn = true;
                verticalColumnEmphasis = GetVerticalColumnBackgroundEmphasis();
            }

            // First drop all sections...ensure the default section is gone
            page.ClearPage();

            // Should not occur, but to be at the safe side...
            if (pageData.Item2.Count == 0)
            {
                page.AddSection(CanvasSectionTemplate.OneColumn, 1, GetBackgroundEmphasis(1));
                return;
            }

            var firstRow = pageData.Item2.OrderBy(p => p.Row).First().Row;
            var lastRow = pageData.Item2.OrderBy(p => p.Row).Last().Row;

            // Loop over the possible rows...will take in account possible row gaps
            // Each row means a new section
            int sectionOrder = 1;
            for (int rowIterator = firstRow; rowIterator <= lastRow; rowIterator++)
            {
                var webpartsInRow = pageData.Item2.Where(p => p.Row == rowIterator);
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

                    if (maxColumns == 0)
                    {
                        maxColumns = 1;
                    }

                    if (maxColumns > 3)
                    {
                        LogError(LogStrings.Error_Maximum3ColumnsAllowed, LogStrings.Heading_PublishingLayoutTransformator);
                        throw new Exception("Publishing transformation layout mapping can maximum use 3 columns");
                    }
                    else
                    {
                        if (maxColumns == 1)
                        {
                            if (includeVerticalColumn && rowIterator == firstRow)
                            {
                                page.AddSection(CanvasSectionTemplate.OneColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                            }
                            else
                            {
                                page.AddSection(CanvasSectionTemplate.OneColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
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
                                                page.AddSection(CanvasSectionTemplate.TwoColumnRightVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnRight, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                        else
                                        {
                                            // image right
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnLeftVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (includeVerticalColumn && rowIterator == firstRow)
                                        {
                                            page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                        }
                                        else
                                        {
                                            page.AddSection(CanvasSectionTemplate.TwoColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
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
                                                page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                        else if (!firstImageColumnOtherWebParts.Any() && secondImageColumnOtherWebParts.Any())
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnRightVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnRight, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                        else if (firstImageColumnOtherWebParts.Any() && !secondImageColumnOtherWebParts.Any())
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnLeftVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnLeft, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                        else
                                        {
                                            if (includeVerticalColumn && rowIterator == firstRow)
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                            }
                                            else
                                            {
                                                page.AddSection(CanvasSectionTemplate.TwoColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (includeVerticalColumn && rowIterator == firstRow)
                                        {
                                            page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                        }
                                        else
                                        {
                                            page.AddSection(CanvasSectionTemplate.TwoColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (includeVerticalColumn && rowIterator == firstRow)
                                {
                                    if (pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnRightVerticalSection)
                                    {
                                        page.AddSection(CanvasSectionTemplate.TwoColumnRightVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                    }
                                    else if (pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnLeftVerticalSection)
                                    {
                                        page.AddSection(CanvasSectionTemplate.TwoColumnLeftVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                    }
                                    else
                                    {
                                        page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                                    }
                                }
                                else
                                {
                                    page.AddSection(CanvasSectionTemplate.TwoColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
                                }
                            }
                        }
                        else if (maxColumns == 3)
                        {
                            if (includeVerticalColumn && rowIterator == firstRow)
                            {
                                page.AddSection(CanvasSectionTemplate.ThreeColumnVerticalSection, sectionOrder, GetBackgroundEmphasis(rowIterator), verticalColumnEmphasis);
                            }
                            else
                            {
                                page.AddSection(CanvasSectionTemplate.ThreeColumn, sectionOrder, GetBackgroundEmphasis(rowIterator));
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
        }

        #region Helper methods
        private int GetBackgroundEmphasis(int row)
        {
            BackgroundEmphasis emphasis = BackgroundEmphasis.None;

            if (this.pageLayoutMappingModel != null)
            {
                if (this.pageLayoutMappingModel.SectionEmphasis != null && this.pageLayoutMappingModel.SectionEmphasis.Section != null)
                {
                    var section = this.pageLayoutMappingModel.SectionEmphasis.Section.Where(p => p.Row == row).FirstOrDefault();
                    if (section != null)
                    {
                        return BackgroundEmphasisToInt(section.Emphasis);
                    }
                }
            }

            return BackgroundEmphasisToInt(emphasis);
        }

        private int GetVerticalColumnBackgroundEmphasis()
        {
            BackgroundEmphasis emphasis = BackgroundEmphasis.None;

            if (this.pageLayoutMappingModel != null)
            {
                if (this.pageLayoutMappingModel.SectionEmphasis != null && this.pageLayoutMappingModel.SectionEmphasis.VerticalColumnEmphasisSpecified)
                {
                    return BackgroundEmphasisToInt(this.pageLayoutMappingModel.SectionEmphasis.VerticalColumnEmphasis);
                }
            }

            return BackgroundEmphasisToInt(emphasis);
        }

        private int BackgroundEmphasisToInt(BackgroundEmphasis emphasis)
        {
            switch (emphasis)
            {
                case BackgroundEmphasis.None: return 0;
                case BackgroundEmphasis.Neutral: return 1;
                case BackgroundEmphasis.Soft: return 2;
                case BackgroundEmphasis.Strong: return 3;
            }

            return 0;
        }
        #endregion
    }
}