using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LFNet.Common.Extensions;

namespace LFNet.Common.Collections
{
    public class ReadOnlyNamedObjectCollection<T> : ReadOnlyCollection<T>, IReadOnlyNamedObjectCollection<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable where T: INamedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.NamedObjectCollection`1" /> class.
        /// </summary>
        /// <param name="items">The items from which the elements are copied.</param>
        public ReadOnlyNamedObjectCollection(IEnumerable<T> items) : base(items.ToList<T>())
        {
        }

        /// <summary>
        /// Determines whether an element is in the collection with the specified name.
        /// </summary>
        /// <param name="name">The name of the item to locate in the collection.</param>
        /// <returns>
        /// <c>true</c> if item is found in the collection; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string name)
        {
            return this.Any<T>(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines the index of a specific item in the list.
        /// </summary>
        /// <param name="name">The name of the item to locate in the list.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(string name)
        {
            return this.IndexOf<T>(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < base.Count; i++)
            {
                builder.Append(base[i]);
                if (i < (base.Count - 1))
                {
                    builder.Append(", ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets the item with the specified name.
        /// </summary>
        /// <returns>
        /// The item with the specified name.
        /// </returns>
        public virtual T this[string name]
        {
            get
            {
                return this.FirstOrDefault<T>(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}

