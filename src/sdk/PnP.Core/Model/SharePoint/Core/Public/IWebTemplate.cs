using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(WebTemplate))]
    public interface IWebTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayCategory { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ImageUrl { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRootWebOnly { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSubWebOnly { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Lcid { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; }
    }
}
