using System.ComponentModel;

namespace LFNet.Common.Collections
{
    public interface INamedObject
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        [Description("The Name of the schema object.")]
        string Name { get; }
    }
}

