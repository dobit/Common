using System.Collections.Generic;
using System.Reflection;

namespace LFNet.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    internal class ObjectMappingInfo
    {
        private const string RootCacheKey = "ObjectCache_";
        private readonly Dictionary<string, string> _columnNames;
        private readonly Dictionary<string, PropertyInfo> _properties;

        public ObjectMappingInfo()
        {
            _properties = new Dictionary<string, PropertyInfo>();
            _columnNames = new Dictionary<string, string>();
        }

        public string CacheKey
        {
            get
            {
                string cacheKey = RootCacheKey + TableName + "_";
                if (!string.IsNullOrEmpty(CacheByProperty))
                {
                    cacheKey += CacheByProperty + "_";
                }
                return cacheKey;
            }
        }

        /// <summary>
        /// Gets or sets the cache by property.
        /// </summary>
        /// <value>The cache by property.</value>
        /// <remarks></remarks>
        public string CacheByProperty { get; set; }

        /// <summary>
        /// Gets or sets the cache time out multiplier.
        /// </summary>
        /// <value>The cache time out multiplier.</value>
        /// <remarks></remarks>
        public int CacheTimeOutMultiplier { get; set; }

        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <remarks></remarks>
        public Dictionary<string, string> ColumnNames
        {
            get { return _columnNames; }
        }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        /// <remarks></remarks>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>The primary key.</value>
        /// <remarks></remarks>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <remarks></remarks>
        public Dictionary<string, PropertyInfo> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        /// <remarks></remarks>
        public string TableName { get; set; }
    }
}