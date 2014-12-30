using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LFNet.Common.Extensions
{
    public static class ValidationExtender
    {
        [DebuggerHidden]
        public static Validation<T> Eval<T>(this Validation<T> item, bool expression)
        {
            if (!expression)
            {
                throw new ArgumentException("Expression evaluated false", item.ArgName);
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<T> Eval<T>(this Validation<T> item, Expression<Func<T, bool>> expression)
        {
            expression.Require<Expression<Func<T, bool>>>("expression").NotNull<Expression<Func<T, bool>>>();
            if (!expression.Compile()(item.Value))
            {
                LambdaExpression expression2 = expression;
                throw new ArgumentException(expression2.Body.ToString().FormatAs("Expression '{0}' evaluated false"), item.ArgName);
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<T> ExistsInList<T>(this Validation<T> item, IList<T> list)
        {
            if (!list.Contains(item.Value))
            {
                throw new ArgumentException("The value {0} did not exist in the provided list of valid values.".FormatWith(new object[] { item.ArgName }), item.ArgName);
            }
            return item;
        }

        public static Validation<T> IsEqualTo<T>(this Validation<T> item, T other) where T: IComparable
        {
            if (item.Value.CompareTo(other) != 0)
            {
                throw new ArgumentOutOfRangeException(item.ArgName, item.Value, "Parameter {0} must be Equal to {1} ".FormatWith(new object[] { item.ArgName, other }));
            }
            return item;
        }

        public static Validation<T> IsGreaterThan<T>(this Validation<T> item, T other) where T: IComparable
        {
            if (item.Value.CompareTo(other) <= 0)
            {
                throw new ArgumentOutOfRangeException(item.ArgName, item.Value, "Parameter {0} must be greater than {1} ".FormatWith(new object[] { item.ArgName, other }));
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<T> IsInRange<T>(this Validation<T> item, T lowerBoundry, T upperBoundry) where T: IComparable
        {
            if ((item.Value.CompareTo(lowerBoundry) < 0) || (item.Value.CompareTo(upperBoundry) > 0))
            {
                throw new ArgumentOutOfRangeException(item.ArgName, item.Value, "Parameter {0} cannot be less than {1} or greater than {2}".FormatWith(new object[] { item.ArgName, lowerBoundry, upperBoundry }));
            }
            return item;
        }

        public static Validation<T> IsLessThan<T>(this Validation<T> item, T other) where T: IComparable
        {
            if (item.Value.CompareTo(other) >= 0)
            {
                throw new ArgumentOutOfRangeException(item.ArgName, item.Value, "Parameter {0} must be less than {1} ".FormatWith(new object[] { item.ArgName, other }));
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<T> NotNull<T>(this Validation<T> item) where T: class
        {
            if (item.Value == null)
            {
                throw new ArgumentNullException(item.ArgName);
            }
            return item;
        }
    }
}

