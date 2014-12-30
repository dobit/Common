using System;
using System.ComponentModel;

namespace LFNet.Common.Collections
{
    /// <summary>
    /// Provides a base for objects with names to derive from.
    /// </summary>
    [Serializable]
    public abstract class NamedObjectBase : INamedObject
    {
        protected string _name;

        public NamedObjectBase()
        {
            this._name = string.Empty;
        }

        public NamedObjectBase(string name)
        {
            this._name = string.Empty;
            this._name = name;
        }

        /// <summary>
        /// Returns the name of the table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// The name of the object.
        /// </summary>
        [Description("The Name of the object.")]
        public virtual string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

