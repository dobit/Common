using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LFNet.Common
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a shorter string value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ShortGuid : IComparable, IComparable<Guid>, IEquatable<Guid>, IComparable<ShortGuid>, IEquatable<ShortGuid>
    {
        /// <summary>
        /// A read-only instance of the ShortGuid class whose value 
        /// is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly ShortGuid Empty;
        private global::System.Guid _guid;
        private string _value;
        /// <summary>
        /// Creates a ShortGuid from a base64 encoded string
        /// </summary>
        /// <param name="value">The encoded guid as a 
        /// base64 string</param>
        public ShortGuid(string value)
        {
            this._value = value;
            this._guid = Decode(value);
        }

        /// <summary>
        /// Creates a ShortGuid from a Guid
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public ShortGuid(global::System.Guid guid)
        {
            this._value = Encode(guid);
            this._guid = guid;
        }

        /// <summary>
        /// Gets/sets the underlying Guid
        /// </summary>
        public global::System.Guid Guid
        {
            get
            {
                return this._guid;
            }
            set
            {
                if (value != this._guid)
                {
                    this._guid = value;
                    this._value = Encode(value);
                }
            }
        }
        /// <summary>
        /// Gets/sets the underlying base64 encoded string
        /// </summary>
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (value != this._value)
                {
                    this._value = value;
                    this._guid = Decode(value);
                }
            }
        }
        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// </returns>
        /// <param name="obj">
        /// An object to compare with this instance. 
        /// </param>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is ShortGuid)
            {
                return this._guid.CompareTo(((ShortGuid) obj)._guid);
            }
            if (obj is global::System.Guid)
            {
                return this._guid.CompareTo((global::System.Guid) obj);
            }
            if (obj is string)
            {
                return this._guid.CompareTo(((ShortGuid) obj)._guid);
            }
            return 0;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this instance. 
        /// </param>
        public int CompareTo(global::System.Guid other)
        {
            return this._guid.CompareTo(other);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this instance. 
        /// </param>
        public int CompareTo(ShortGuid other)
        {
            return this._guid.CompareTo(other._guid);
        }

        /// <summary>
        /// Returns the base64 encoded guid as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
            {
                return this._guid.Equals(((ShortGuid) obj)._guid);
            }
            if (obj is global::System.Guid)
            {
                return this._guid.Equals((global::System.Guid) obj);
            }
            return ((obj is string) && this._guid.Equals(((ShortGuid) obj)._guid));
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="other">The object to compare</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ShortGuid other)
        {
            return this._guid.Equals(other._guid);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="other">The object to compare</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(global::System.Guid other)
        {
            return this._guid.Equals(other);
        }

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this._guid.GetHashCode();
        }

        /// <summary>
        /// Initialises a new instance of the ShortGuid class
        /// </summary>
        /// <returns></returns>
        public static ShortGuid NewGuid()
        {
            return new ShortGuid(global::System.Guid.NewGuid());
        }

        /// <summary>
        /// Creates a new instance of a Guid using the string value, 
        /// then returns the base64 encoded version of the Guid.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a ShortGuid)</param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            global::System.Guid guid = new global::System.Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base64 string that is 22 
        /// characters long.
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        /// <returns></returns>
        public static string Encode(global::System.Guid guid)
        {
            return EncodeBase62(Convert.ToBase64String(guid.ToByteArray()).Substring(0, 0x16));
        }

        private static string EncodeBase62(string base64)
        {
            StringBuilder builder = new StringBuilder(base64.Length * 2);
            for (int i = 0; i < base64.Length; i++)
            {
                char ch = base64[i];
                char ch2 = ch;
                if (ch2 <= '+')
                {
                    switch (ch2)
                    {
                        case '\n':
                        {
                            continue;
                        }
                        case '+':
                            goto Label_004A;
                    }
                    goto Label_0074;
                }
                switch (ch2)
                {
                    case '/':
                    {
                        builder.Append("is");
                        continue;
                    }
                    case '=':
                    {
                        builder.Append("ie");
                        continue;
                    }
                    default:
                    {
                        if (ch2 != 'i')
                        {
                            goto Label_0074;
                        }
                        builder.Append("ii");
                        continue;
                    }
                }
            Label_004A:
                builder.Append("ip");
                continue;
            Label_0074:
                builder.Append(ch);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Decodes the given base64 string
        /// </summary>
        /// <param name="value">The base64 encoded string of a Guid</param>
        /// <returns>A new Guid</returns>
        public static global::System.Guid Decode(string value)
        {
            value = DecodeBase62(value);
            return new global::System.Guid(Convert.FromBase64String(value + "=="));
        }

        private static string DecodeBase62(string base62)
        {
            StringBuilder builder = new StringBuilder(base62.Length);
            for (int i = 0; i < base62.Length; i++)
            {
                char ch = base62[i];
                if (ch == 'i')
                {
                    i++;
                    switch (base62[i])
                    {
                        case 'p':
                            builder.Append('+');
                            break;

                        case 's':
                            builder.Append('/');
                            break;

                        case 'e':
                            builder.Append('=');
                            break;

                        case 'i':
                            builder.Append('i');
                            break;
                    }
                }
                else
                {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Determines if both ShortGuids have the same underlying 
        /// Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            if (x ==ShortGuid.Empty)
            {
                return y == ShortGuid.Empty;
            }
            return x._guid == y._guid;
        }

        /// <summary>
        /// Determines if both ShortGuids do not have the 
        /// same underlying Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(ShortGuid x, ShortGuid y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the ShortGuid to it's string equivilent
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator string(ShortGuid shortGuid)
        {
            return shortGuid._value;
        }

        /// <summary>
        /// Implicitly converts the ShortGuid to it's Guid equivilent
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator global::System.Guid(ShortGuid shortGuid)
        {
            return shortGuid._guid;
        }

        /// <summary>
        /// Implicitly converts the string to a ShortGuid
        /// </summary>
        /// <param name="shortGuid"></param>
        /// <returns></returns>
        public static implicit operator ShortGuid(string shortGuid)
        {
            return new ShortGuid(shortGuid);
        }

        /// <summary>
        /// Implicitly converts the Guid to a ShortGuid 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static implicit operator ShortGuid(global::System.Guid guid)
        {
            return new ShortGuid(guid);
        }

        static ShortGuid()
        {
            Empty = new ShortGuid(global::System.Guid.Empty);
        }
    }
}

