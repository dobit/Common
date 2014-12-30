using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace System.Collections.Concurrent.Partitioners
{
    /// <summary>
    /// Partitions an enumerable into chunks based on user-supplied criteria.
    /// </summary>
    public static class ChunkPartitioner
    {
        /// <summary>Creates a partitioner that chooses the next chunk size based on a user-supplied function.</summary>
        /// <typeparam name="TSource">The type of the data being partitioned.</typeparam>
        /// <param name="source">The data being partitioned.</param>
        /// <param name="nextChunkSizeFunc">A function that determines the next chunk size based on the
        /// previous chunk size.</param>
        /// <returns>A partitioner.</returns>
        public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source, Func<int, int> nextChunkSizeFunc)
        {
            return new ChunkPartitioner<TSource>(source, nextChunkSizeFunc);
        }

        /// <summary>Creates a partitioner that always uses a user-specified chunk size.</summary>
        /// <typeparam name="TSource">The type of the data being partitioned.</typeparam>
        /// <param name="source">The data being partitioned.</param>
        /// <param name="chunkSize">The chunk size to be used.</param>
        /// <returns>A partitioner.</returns>
        public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source, int chunkSize)
        {
            return new ChunkPartitioner<TSource>(source, chunkSize);
        }

        /// <summary>Creates a partitioner that chooses chunk sizes between the user-specified min and max.</summary>
        /// <typeparam name="TSource">The type of the data being partitioned.</typeparam>
        /// <param name="source">The data being partitioned.</param>
        /// <param name="minChunkSize">The minimum chunk size to use.</param>
        /// <param name="maxChunkSize">The maximum chunk size to use.</param>
        /// <returns>A partitioner.</returns>
        public static OrderablePartitioner<TSource> Create<TSource>(IEnumerable<TSource> source, int minChunkSize, int maxChunkSize)
        {
            return new ChunkPartitioner<TSource>(source, minChunkSize, maxChunkSize);
        }
    }
}

