using System;

namespace LFNet.Common.CommandLine
{
    /// <summary>
    /// Allows control of command line parsing.
    /// Attach this attribute to instance fields of types used
    /// as the destination of command line argument parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgumentAttribute : Attribute
    {
        private object defaultValue;
        private string helpText;
        private bool isToggle;
        private string longName;
        private string shortName;
        private ArgumentType type;

        /// <summary>
        /// Allows control of command line parsing.
        /// </summary>
        /// <param name="type"> Specifies the error checking to be done on the argument. </param>
        public ArgumentAttribute(ArgumentType type)
        {
            this.type = type;
        }

        /// <summary>
        /// Returns true if the argument did not have an explicit long name specified.
        /// </summary>
        public bool DefaultLongName
        {
            get
            {
                return (null == this.longName);
            }
        }

        /// <summary>
        /// Returns true if the argument did not have an explicit short name specified.
        /// </summary>
        public bool DefaultShortName
        {
            get
            {
                return (null == this.shortName);
            }
        }

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        /// <summary>
        /// Returns true if the argument has a default value.
        /// </summary>
        public bool HasDefaultValue
        {
            get
            {
                return (null != this.defaultValue);
            }
        }

        /// <summary>
        /// Returns true if the argument has help text specified.
        /// </summary>
        public bool HasHelpText
        {
            get
            {
                return (null != this.helpText);
            }
        }

        /// <summary>
        /// The help text for the argument.
        /// </summary>
        public string HelpText
        {
            get
            {
                return this.helpText;
            }
            set
            {
                this.helpText = value;
            }
        }

        /// <summary>
        /// Show optional boolean toggle syntax options for the arguments.  
        /// </summary>
        public bool IsToggle
        {
            get
            {
                return this.isToggle;
            }
            set
            {
                this.isToggle = value;
            }
        }

        /// <summary>
        /// The long name of the argument.
        /// Set to null means use the default long name.
        /// The long name for every argument must be unique.
        /// It is an error to specify a long name of String.Empty.
        /// </summary>
        public string LongName
        {
            get
            {
                return this.longName;
            }
            set
            {
                this.longName = value;
            }
        }

        /// <summary>
        /// The short name of the argument.
        /// Set to null means use the default short name if it does not
        /// conflict with any other parameter name.
        /// Set to String.Empty for no short name.
        /// This property should not be set for DefaultArgumentAttributes.
        /// </summary>
        public string ShortName
        {
            get
            {
                return this.shortName;
            }
            set
            {
                this.shortName = value;
            }
        }

        /// <summary>
        /// The error checking to be done on the argument.
        /// </summary>
        public ArgumentType Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

