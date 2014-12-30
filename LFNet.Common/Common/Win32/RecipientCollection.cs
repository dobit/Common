using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// A collection of recipients for a mail message.
    /// </summary>
    public class RecipientCollection : Collection<Recipient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.RecipientCollection" /> class.
        /// </summary>
        public RecipientCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.RecipientCollection" /> class.
        /// </summary>
        /// <param name="list">The list that is wrapped by the new collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="list" /> is <c>null</c>.
        /// </exception>
        public RecipientCollection(IList<Recipient> list) : base(list)
        {
        }

        /// <summary>
        /// Adds a new recipient with the specified email address to this collection.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public void Add(string emailAddress)
        {
            base.Add(new Recipient(emailAddress));
        }

        /// <summary>
        /// Adds a new recipient with the specified email address and recipient type to this collection.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="recipientType">Type of the recipient.</param>
        public void Add(string emailAddress, MailDialog.RecipientType recipientType)
        {
            base.Add(new Recipient(emailAddress, recipientType));
        }

        /// <summary>
        /// Adds a new recipient with the specified email address and display name to this collection.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="displayName">The display name.</param>
        public void Add(string emailAddress, string displayName)
        {
            base.Add(new Recipient(emailAddress, displayName));
        }

        /// <summary>
        /// Adds a new recipient with the specified email address, display name and recipient type to this collection.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="recipientType">Type of the recipient.</param>
        public void Add(string emailAddress, string displayName, MailDialog.RecipientType recipientType)
        {
            base.Add(new Recipient(emailAddress, displayName, recipientType));
        }

        internal RecipientCollectionHandle GetHandle()
        {
            return new RecipientCollectionHandle(this);
        }

        internal class RecipientCollectionHandle : CriticalHandle
        {
            private int _count;

            public RecipientCollectionHandle(ICollection<Recipient> list) : base(IntPtr.Zero)
            {
                if (list == null)
                {
                    throw new ArgumentNullException("list");
                }
                this._count = list.Count;
                if (this._count != 0)
                {
                    Type t = typeof(Mapi.MapiRecipDesc);
                    int num = Marshal.SizeOf(t);
                    base.SetHandle(Marshal.AllocHGlobal((int) (this._count * num)));
                    int handle = (int) base.handle;
                    foreach (Recipient recipient in list)
                    {
                        Marshal.StructureToPtr(recipient.GetMapiRecipDesc(), (IntPtr) handle, false);
                        handle += num;
                    }
                }
            }

            public static implicit operator IntPtr(RecipientCollection.RecipientCollectionHandle recipientCollectionHandle)
            {
                return recipientCollectionHandle.handle;
            }

            protected override bool ReleaseHandle()
            {
                if (!this.IsInvalid)
                {
                    Type t = typeof(Mapi.MapiRecipDesc);
                    int num = Marshal.SizeOf(t);
                    int handle = (int) base.handle;
                    for (int i = 0; i < this._count; i++)
                    {
                        Marshal.DestroyStructure((IntPtr) handle, t);
                        handle += num;
                    }
                    Marshal.FreeHGlobal(base.handle);
                    base.SetHandle(IntPtr.Zero);
                    this._count = 0;
                }
                return true;
            }

            public override bool IsInvalid
            {
                get
                {
                    return (base.handle == IntPtr.Zero);
                }
            }
        }
    }
}

