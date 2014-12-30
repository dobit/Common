using System.Collections.Generic;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// Settings class for the <see cref="T:LFNet.Common.Reflection.ObjectCopier" />.
    /// </summary>
    public class ObjectCopierSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Reflection.ObjectCopierSettings" /> class.
        /// </summary>
        public ObjectCopierSettings()
        {
            this.SuppressExceptions = false;
            this.UseDynamicCache = true;
            this.IgnoreList = new List<string>();
        }

        /// <summary>
        /// Gets or sets the list of property names to ignore when <see cref="T:LFNet.Common.Reflection.ObjectCopier" /> is copying properties.
        /// </summary>
        /// <value>The ignore list.</value>
        public IList<string> IgnoreList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="T:LFNet.Common.Reflection.ObjectCopier" /> will suppress exceptions when copying.
        /// </summary>
        /// <value><c>true</c> to suppress exceptions; otherwise, <c>false</c>.</value>
        public bool SuppressExceptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="T:LFNet.Common.Reflection.ObjectCopier" /> will use dynamic cache.
        /// </summary>
        /// <value><c>true</c> to use dynamic cache; otherwise, <c>false</c>.</value>
        public bool UseDynamicCache { get; set; }
    }
}

