using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LFNet.Common.Extensions;

namespace LFNet.Common.Collections
{
    /// <summary>
    /// A collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary>
    [Serializable, Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), TypeConverter(typeof(DelimitedStringCollection.DelimitedStringTypeConverter))]
    public class DelimitedStringCollection : Collection<string>, INotifyPropertyChanged
    {
        private readonly SimpleMonitor _monitor;
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        /// <summary>
        /// Occurs when the collection changed.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changed.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.DelimitedStringCollection" /> class.
        /// </summary>
        public DelimitedStringCollection()
        {
            this._monitor = new SimpleMonitor();
            this.Delimiter = '|';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.DelimitedStringCollection" /> class.
        /// </summary>
        /// <param name="delimitedValues">The delimited values.</param>
        public DelimitedStringCollection(string delimitedValues) : this()
        {
            this.CopyFrom(delimitedValues);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Collections.DelimitedStringCollection" /> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public DelimitedStringCollection(IEnumerable<string> collection) : this()
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.CopyFrom(collection);
        }

        /// <summary>
        /// Adds the split values from <paramref name="delimitedValues" /> to the collection.
        /// The string is split using the <see cref="P:LFNet.Common.Collections.DelimitedStringCollection.Delimiter" /> property.
        /// </summary>
        /// <param name="delimitedValues">The delimited values.</param>
        /// <returns>The number of values added.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="delimitedValues" /> is null.</exception>
        public int AddDelimited(string delimitedValues)
        {
            this.CheckReentrancy();
            int num = this.CopyFrom(delimitedValues);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
            return num;
        }

        /// <summary>
        /// Disallows reentrant attempts to change this collection.
        /// </summary>
        /// <returns>An IDisposable object that can be used to dispose of the object.</returns>
        protected IDisposable BlockReentrancy()
        {
            this._monitor.Enter();
            return this._monitor;
        }

        /// <summary>
        /// Checks for reentrant attempts to change this collection.
        /// </summary>
        protected void CheckReentrancy()
        {
            if ((this._monitor.Busy && (this.CollectionChanged != null)) && (this.CollectionChanged.GetInvocationList().Length > 1))
            {
                throw new InvalidOperationException("Observable collection reentrancy not allowed.");
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        protected override void ClearItems()
        {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
        }

        private int CopyFrom(string delimitedValues)
        {
            if (string.IsNullOrEmpty(delimitedValues))
            {
                return 0;
            }
            int count = base.Count;
            IEnumerable<string> collection = Parse(delimitedValues, new char[] { this.Delimiter });
            this.CopyFrom(collection);
            return (base.Count - count);
        }

        private void CopyFrom(IEnumerable<string> collection)
        {
            IList<string> items = base.Items;
            if ((collection != null) && (items != null))
            {
                using (IEnumerator<string> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.
        /// -or-
        /// <paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
        /// </exception>
        protected override void InsertItem(int index, string item)
        {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        public void Move(int oldIndex, int newIndex)
        {
            this.MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckReentrancy();
            string item = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Raises the CollectionChanged event with the provided arguments.
        /// </summary>
        protected virtual void OnCollectionChanged()
        {
            if (this.CollectionChanged != null)
            {
                using (this.BlockReentrancy())
                {
                    this.CollectionChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Parses the specified delimited values.
        /// </summary>
        /// <param name="delimitedValues">The delimited values.</param>
        /// <param name="delimiter">The list delimiters.</param>
        /// <returns></returns>
        public static IEnumerable<string> Parse(string delimitedValues, params char[] delimiter)
        {
            if (delimitedValues.IsNullOrEmpty())
            {
                throw new ArgumentNullException("delimitedValues");
            }
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }
            if (delimiter.Length == 0)
            {
                throw new ArgumentOutOfRangeException("delimiter", "There must be at least on delimiter specified.");
            }
            StringBuilder iteratorVariable0 = new StringBuilder();
            StringReader iteratorVariable1 = new StringReader(delimitedValues);
            bool iteratorVariable2 = false;
            do
            {
                char c = (char) iteratorVariable1.Read();
                if ((iteratorVariable0.Length != 0) || (c != ' '))
                {
                    if ((iteratorVariable2 && (c == '"')) && (iteratorVariable1.Peek() == 0x22))
                    {
                        iteratorVariable0.Append(c);
                        iteratorVariable1.Read();
                    }
                    else if (c == '"')
                    {
                        iteratorVariable2 = !iteratorVariable2;
                    }
                    else if (!iteratorVariable2 && delimiter.Any<char>(x => (x == c)))
                    {
                        yield return iteratorVariable0.ToString();
                        iteratorVariable0.Length = 0;
                    }
                    else
                    {
                        iteratorVariable0.Append(c);
                    }
                }
            }
            while (iteratorVariable1.Peek() != -1);
            yield return iteratorVariable0.ToString();
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.
        /// -or-
        /// <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
        /// </exception>
        protected override void RemoveItem(int index)
        {
            this.CheckReentrancy();
            base.RemoveItem(index);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than zero.
        /// -or-
        /// <paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.
        /// </exception>
        protected override void SetItem(int index, string item)
        {
            this.CheckReentrancy();
            base.SetItem(index, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged();
        }

        /// <summary>
        /// Returns an array of strings that represents the current collection.
        /// </summary>
        /// <returns>An array of strings that represents the current collection.</returns>
        public string[] ToArray()
        {
            string[] array = new string[base.Count];
            base.CopyTo(array, 0);
            return array;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return ToString(base.Items, this.Delimiter);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public static string ToString(IEnumerable<string> values)
        {
            return ToString(values, '|');
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public static string ToString(IEnumerable<string> values, char delimiter)
        {
            Func<char, bool> predicate = null;
            StringBuilder builder = new StringBuilder();
            foreach (string str in values)
            {
                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }
                if (predicate == null)
                {
                    predicate = delegate (char c) {
                        if (c != '"')
                        {
                            return c == delimiter;
                        }
                        return true;
                    };
                }
                if (str.Any<char>(predicate))
                {
                    builder.Append('"');
                    builder.Append(str.Replace("\"", "\"\""));
                    builder.Append('"');
                }
                else
                {
                    builder.Append(str);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets or sets the string delimiter.
        /// </summary>
        /// <value>The string delimiter.</value>
        [DefaultValue('|')]
        public char Delimiter { get; set; }


        /// <summary>
        /// A type converter for the <see cref="T:LFNet.Common.Collections.DelimitedStringCollection" />.
        /// </summary>
        public class DelimitedStringTypeConverter : TypeConverter
        {
            /// <summary>
            /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
            /// <returns>
            /// true if this converter can perform the conversion; otherwise, false.
            /// </returns>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            /// <summary>
            /// Returns whether this converter can convert the object to the specified type, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
            /// <returns>
            /// true if this converter can perform the conversion; otherwise, false.
            /// </returns>
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
            }

            /// <summary>
            /// Converts the given object to the type of this converter, using the specified context and culture information.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
            /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
            /// <returns>
            /// An <see cref="T:System.Object" /> that represents the converted value.
            /// </returns>
            /// <exception cref="T:System.NotSupportedException">
            /// The conversion cannot be performed.
            /// </exception>
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                DelimitedStringCollection strings;
                if (!(value is string))
                {
                    return base.ConvertFrom(context, culture, value);
                }
                if (context.PropertyDescriptor.IsReadOnly)
                {
                    strings = context.PropertyDescriptor.GetValue(context.Instance) as DelimitedStringCollection;
                    if (strings != null)
                    {
                        strings.AddDelimited(value.ToString());
                    }
                }
                else
                {
                    strings = new DelimitedStringCollection(value.ToString());
                }
                return (strings ?? base.ConvertFrom(context, culture, value));
            }

            /// <summary>
            /// Converts the given value object to the specified type, using the specified context and culture information.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
            /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
            /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
            /// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
            /// <returns>
            /// An <see cref="T:System.Object" /> that represents the converted value.
            /// </returns>
            /// <exception cref="T:System.ArgumentNullException">
            /// The <paramref name="destinationType" /> parameter is null.
            /// </exception>
            /// <exception cref="T:System.NotSupportedException">
            /// The conversion cannot be performed.
            /// </exception>
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                DelimitedStringCollection strings = value as DelimitedStringCollection;
                if (!(destinationType != typeof(string)) && (strings != null))
                {
                    return strings.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public void Dispose()
            {
                this._busyCount--;
            }

            public void Enter()
            {
                this._busyCount++;
            }

            public bool Busy
            {
                get
                {
                    return (this._busyCount > 0);
                }
            }
        }
    }
}

