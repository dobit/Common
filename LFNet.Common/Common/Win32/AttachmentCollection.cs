using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;

namespace LFNet.Common.Win32
{
    /// <summary>
    /// A collection class for attachments
    /// </summary>
    public class AttachmentCollection : Collection<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.AttachmentCollection" /> class.
        /// </summary>
        public AttachmentCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Win32.AttachmentCollection" /> class.
        /// </summary>
        /// <param name="list">The list that is wrapped by the new collection.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="list" /> is null.
        /// </exception>
        public AttachmentCollection(IList<string> list) : base(list)
        {
        }

        internal AttachmentCollectionHandle GetHandle()
        {
            return new AttachmentCollectionHandle(this);
        }

        internal class AttachmentCollectionHandle : CriticalHandle
        {
            private int _count;

            public AttachmentCollectionHandle(ICollection<string> files) : base(IntPtr.Zero)
            {
                if (files == null)
                {
                    throw new ArgumentNullException("files");
                }
                this._count = files.Count;
                if (this._count != 0)
                {
                    Type t = typeof(Mapi.MapiFileDescriptor);
                    int num = Marshal.SizeOf(t);
                    base.SetHandle(Marshal.AllocHGlobal((int) (this._count * num)));
                    int handle = (int) base.handle;
                    foreach (string str in files)
                    {
                        Mapi.MapiFileDescriptor structure = new Mapi.MapiFileDescriptor {
                            Position = -1,
                            FileName = Path.GetFileName(str),
                            PathName = Path.GetFullPath(str)
                        };
                        Marshal.StructureToPtr(structure, (IntPtr) handle, false);
                        handle += num;
                    }
                }
            }

            public static implicit operator IntPtr(AttachmentCollection.AttachmentCollectionHandle recipientCollectionHandle)
            {
                return recipientCollectionHandle.handle;
            }

            protected override bool ReleaseHandle()
            {
                if (!this.IsInvalid)
                {
                    Type t = typeof(Mapi.MapiFileDescriptor);
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

