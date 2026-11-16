using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Pages;
using System;
using System.Collections.Generic;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Transform
{

    /// <summary>
    /// Transforms the layout of a classic wiki/webpart page into a modern client side page using sections and columns
    /// </summary>
    public class LayoutTransformator: ILayoutTransformator
    {
        private PnPCore.IPage page;

        #region Construction
        /// <summary>
        /// Creates a layout transformator instance
        /// </summary>
        /// <param name="page">Client side page that will be receive the created layout</param>
        public LayoutTransformator(PnPCore.IPage page)
        {
            this.page = page;
        }
        #endregion

        /// <summary>
        /// Transforms a classic wiki/webpart page layout into a modern client side page layout
        /// </summary>
        /// <param name="pageData">Information about the analyed page</param>
        public virtual void Transform(Tuple<Pages.PageLayout, List<WebPartEntity>> pageData)
        {
            switch (pageData.Item1)
            {
                // In case of a custom layout let's stick with one column as model
                case PageLayout.Wiki_OneColumn:
                case PageLayout.WebPart_FullPageVertical:
                case PageLayout.Wiki_Custom:
                case PageLayout.WebPart_Custom:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        return;
                    }
                case PageLayout.Wiki_TwoColumns:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumn, 1);
                        return;
                    }
                case PageLayout.Wiki_ThreeColumns:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.ThreeColumn, 1);
                        return;
                    }
                case PageLayout.Wiki_TwoColumnsWithSidebar:
                case PageLayout.WebPart_2010_TwoColumnsLeft:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumnLeft, 1);
                        return;
                    }
                case PageLayout.WebPart_HeaderRightColumnBody:
                case PageLayout.WebPart_DocumentWorkspace:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumnLeft, 2);
                        return;
                    }
                case PageLayout.WebPart_HeaderLeftColumnBody:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumnRight, 2);
                        return;
                    }
                case PageLayout.Wiki_TwoColumnsWithHeader:
                case PageLayout.WebPart_ProjectSite:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumn, 2);
                        return;
                    }
                case PageLayout.WebPart_DocumentCenter:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumnLeft, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumn, 2);
                        return;
                    }
                case PageLayout.Wiki_TwoColumnsWithHeaderAndFooter:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.TwoColumn, 2);
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 3);
                        return;
                    }
                case PageLayout.Wiki_ThreeColumnsWithHeader:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.ThreeColumn, 2);
                        return;
                    }
                case PageLayout.Wiki_ThreeColumnsWithHeaderAndFooter:
                case PageLayout.WebPart_HeaderFooterThreeColumns:
                case PageLayout.WebPart_HeaderFooter4ColumnsTopRow:
                case PageLayout.WebPart_HeaderFooter2Columns4Rows:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.ThreeColumn, 2);
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 3);
                        return;
                    }
                case PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns:
                case PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 2);
                        page.AddSection(PnPCore.CanvasSectionTemplate.ThreeColumn, 3);
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 4);
                        return;
                    }
                default:
                    {
                        page.AddSection(PnPCore.CanvasSectionTemplate.OneColumn, 1);
                        return;
                    }
            }
        }

    }
}
