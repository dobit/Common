namespace LFNet.Common.Win32
{
    /// <summary>
    /// Represents a Recipient for a <see cref="T:LFNet.Common.Win32.MailDialog" />.
    /// </summary>
    public class Recipient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.Recipient" /> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public Recipient(string emailAddress) : this(emailAddress, emailAddress, MailDialog.RecipientType.To)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.Recipient" /> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="recipientType">Type of the recipient.</param>
        public Recipient(string emailAddress, MailDialog.RecipientType recipientType) : this(emailAddress, emailAddress, recipientType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.Recipient" /> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="displayName">The display name.</param>
        public Recipient(string emailAddress, string displayName) : this(emailAddress, displayName, MailDialog.RecipientType.To)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.Recipient" /> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="recipientType">Type of the recipient.</param>
        public Recipient(string emailAddress, string displayName, MailDialog.RecipientType recipientType)
        {
            this.EmailAddress = emailAddress;
            this.DisplayName = displayName;
            this.RecipientType = recipientType;
        }

        internal Mapi.MapiRecipDesc GetMapiRecipDesc()
        {
            return new Mapi.MapiRecipDesc { Name = this.DisplayName ?? this.EmailAddress, Address = this.EmailAddress, RecipientClass = (int) this.RecipientType };
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>The email address.</value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the type of the recipient.
        /// </summary>
        /// <value>The type of the recipient.</value>
        public MailDialog.RecipientType RecipientType { get; set; }
    }
}

