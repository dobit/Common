using System.Collections.Generic;
using System.Threading;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>A work-stealing queue.</summary>
    /// <typeparam name="T">Specifies the type of data stored in the queue.</typeparam>
    internal class WorkStealingQueue<T> where T: class
    {
        private const int INITIAL_SIZE = 0x20;
        private T[] m_array;
        private object m_foreignLock;
        private volatile int m_headIndex;
        private int m_mask;
        private volatile int m_tailIndex;

        public WorkStealingQueue()
        {
            this.m_array = new T[0x20];
            this.m_mask = 0x1f;
            this.m_foreignLock = new object();
        }

        internal bool LocalPop(ref T obj)
        {
            int num;
        Label_0000:
            num = this.m_tailIndex;
            if (this.m_headIndex >= num)
            {
                obj = default(T);
                return false;
            }
            num--;
            Interlocked.Exchange(ref this.m_tailIndex, num);
            if (this.m_headIndex <= num)
            {
                int index = num & this.m_mask;
                obj = this.m_array[index];
                if (((T) obj) != null)
                {
                    this.m_array[index] = default(T);
                    return true;
                }
                goto Label_0000;
            }
            lock (this.m_foreignLock)
            {
                if (this.m_headIndex <= num)
                {
                    int num3 = num & this.m_mask;
                    obj = this.m_array[num3];
                    if (((T) obj) != null)
                    {
                        this.m_array[num3] = default(T);
                        return true;
                    }
                    goto Label_0000;
                }
                this.m_tailIndex = num + 1;
                obj = default(T);
                return false;
            }
        }

        internal void LocalPush(T obj)
        {
            int tailIndex = this.m_tailIndex;
            if (tailIndex < (this.m_headIndex + this.m_mask))
            {
                this.m_array[tailIndex & this.m_mask] = obj;
                this.m_tailIndex = tailIndex + 1;
            }
            else
            {
                lock (this.m_foreignLock)
                {
                    int headIndex = this.m_headIndex;
                    int num3 = this.m_tailIndex - this.m_headIndex;
                    if (num3 >= this.m_mask)
                    {
                        T[] localArray = new T[this.m_array.Length << 1];
                        for (int i = 0; i < this.m_array.Length; i++)
                        {
                            localArray[i] = this.m_array[(i + headIndex) & this.m_mask];
                        }
                        this.m_array = localArray;
                        this.m_headIndex = 0;
                        this.m_tailIndex = tailIndex = num3;
                        this.m_mask = (this.m_mask << 1) | 1;
                    }
                    this.m_array[tailIndex & this.m_mask] = obj;
                    this.m_tailIndex = tailIndex + 1;
                }
            }
        }

        internal T[] ToArray()
        {
            List<T> list = new List<T>();
            for (int i = this.m_tailIndex - 1; i >= this.m_headIndex; i--)
            {
                T item = this.m_array[i & this.m_mask];
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        internal bool TryFindAndPop(T obj)
        {
            for (int i = this.m_tailIndex - 1; i >= this.m_headIndex; i--)
            {
                if (this.m_array[i & this.m_mask] == obj)
                {
                    lock (this.m_foreignLock)
                    {
                        if (this.m_array[i & this.m_mask] == null)
                        {
                            return false;
                        }
                        T local = default(T);
                        this.m_array[i & this.m_mask] = local;
                        if (i == this.m_tailIndex)
                        {
                            this.m_tailIndex--;
                        }
                        else if (i == this.m_headIndex)
                        {
                            this.m_headIndex++;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool TrySteal(ref T obj)
        {
            obj = default(T);
        Label_0007:
            if (this.m_headIndex < this.m_tailIndex)
            {
                lock (this.m_foreignLock)
                {
                    int headIndex = this.m_headIndex;
                    Interlocked.Exchange(ref this.m_headIndex, headIndex + 1);
                    if (headIndex < this.m_tailIndex)
                    {
                        int index = headIndex & this.m_mask;
                        obj = this.m_array[index];
                        if (((T) obj) != null)
                        {
                            this.m_array[index] = default(T);
                            return true;
                        }
                        goto Label_0007;
                    }
                    this.m_headIndex = headIndex;
                    obj = default(T);
                }
            }
            return false;
        }
    }
}

