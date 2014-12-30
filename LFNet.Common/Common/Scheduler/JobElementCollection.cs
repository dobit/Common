using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A configuration element collection class for <see cref="T:LFNet.Common.Scheduler.JobElement" />.
    /// </summary>
    public class JobElementCollection : ConfigurationElementCollection, IEnumerable<IJobConfiguration>, IEnumerable
    {
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new JobElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            JobElement element2 = element as JobElement;
            if (element2 == null)
            {
                throw new ArgumentException("The specified element is not of the correct type.");
            }
            return element2.Name;
        }

        IEnumerator<IJobConfiguration> IEnumerable<IJobConfiguration>.GetEnumerator()
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                IJobConfiguration current = (IJobConfiguration) enumerator.Current;
                yield return current;
            }
        }

    }
}

