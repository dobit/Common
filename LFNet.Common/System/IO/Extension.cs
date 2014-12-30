namespace System.IO
{
   public static class Extension
    {
#if NET3
       public static void CopyTo(this Stream source, Stream destination)
       {
           if (destination == null)
           {
               throw new ArgumentNullException("destination");
           }
           if (!source.CanRead && !source.CanWrite)
           {
               throw new ObjectDisposedException(null, "ObjectDisposed Or StreamClosed");
           }
           if (!destination.CanRead && !destination.CanWrite)
           {
               throw new ObjectDisposedException("destination", "ObjectDisposed Or StreamClosed");
           }
           if (!source.CanRead)
           {
               throw new NotSupportedException("UnreadableStream");
           }
           if (!destination.CanWrite)
           {
               throw new NotSupportedException("UnwritableStream");
           }
           source.InternalCopyTo(destination, 81920);
       }
       public static void CopyTo(this Stream source, Stream destination, int bufferSize)
       {
           if (destination == null)
           {
               throw new ArgumentNullException("destination");
           }
           if (bufferSize <= 0)
           {
               throw new ArgumentOutOfRangeException("bufferSize", "ArgumentOutOfRange NeedPosNum");
           }
           if (!source.CanRead && !source.CanWrite)
           {
               throw new ObjectDisposedException(null, "ObjectDisposed Or StreamClosed");
           }
           if (!destination.CanRead && !destination.CanWrite)
           {
               throw new ObjectDisposedException("destination", "ObjectDisposed Or StreamClosed");
           }
           if (!source.CanRead)
           {
               throw new NotSupportedException("UnreadableStream");
           }
           if (!destination.CanWrite)
           {
               throw new NotSupportedException("UnwritableStream");
           }
           source.InternalCopyTo(destination, bufferSize);
       }
       private static void InternalCopyTo(this Stream source,Stream destination, int bufferSize)
       {
           byte[] array = new byte[bufferSize];
           int count;
           while ((count = source.Read(array, 0, array.Length)) != 0)
           {
               destination.Write(array, 0, count);
           }
       }
#endif

    }
}
