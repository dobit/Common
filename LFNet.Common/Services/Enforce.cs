using System;

namespace LFNet.Services
{
    /// <summary>
    /// Helper methods used throughout the codebase.
    /// </summary>
    static class Enforce
    {
        /// <summary>
        /// Enforce that an argument is not null. Returns the
        /// value if valid so that it can be used inline in
        /// base initialiser syntax.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns><paramref name="value"/></returns>
        public static T ArgumentNotNull<T>(T value, string name)
            where T : class
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (value == null)
                throw new ArgumentNullException(name);

            return value;
        }
    }
}