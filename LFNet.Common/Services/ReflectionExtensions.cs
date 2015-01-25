

using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;

namespace LFNet.Services
{
    /// <summary>
    /// Extension methods for reflection-related types.
    /// </summary>
    static class ReflectionExtensions
    {
        /// <summary>
        /// Convert to the Autofac parameter type.
        /// </summary>
        /// <returns>The parameters represented by this collection.</returns>
        public static IEnumerable<Parameter> ToParameters(this List<ParameterElement> parameterElements)
        {
            foreach (var parameter in parameterElements)
            {
                var localParameter = parameter;
                yield return new ResolvedParameter(
                    (pi, c) => pi.Name == localParameter.Name,
                    (pi, c) => TypeManipulation.ChangeToCompatibleType(localParameter.Value, pi.ParameterType));
            }
        }

        /// <summary>
        /// Convert to the Autofac parameter type.
        /// </summary>
        /// <returns>The parameters represented by this collection.</returns>
        public static IEnumerable<Parameter> ToParameters(this List<PropertyElement> propertyElements)
        {
            foreach (var parameter in propertyElements)
            {
                var localParameter = parameter;
                yield return new ResolvedParameter(
                    (pi, c) =>
                        {
                            PropertyInfo prop;
                            return pi.TryGetDeclaringProperty(out prop) &&
                                   prop.Name == localParameter.Name;
                        },
                    (pi, c) => TypeManipulation.ChangeToCompatibleType(localParameter.Value, pi.ParameterType));
            }
        }

        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <param name="pi">Parameter to the property setter.</param>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public static bool TryGetDeclaringProperty(this ParameterInfo pi, out PropertyInfo prop)
        {
            var mi = pi.Member as MethodInfo;
            if (mi != null && mi.IsSpecialName && mi.Name.StartsWith("set_"))
            {
                prop = mi.DeclaringType.GetProperty(mi.Name.Substring(4));
                return true;
            }

            prop = null;
            return false;
        }
    }
}
