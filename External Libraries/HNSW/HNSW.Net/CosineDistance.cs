﻿// <copyright file="CosineDistance.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace HNSW.Net
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Calculates cosine similarity.
    /// </summary>
    /// <remarks>
    /// Intuition behind selecting float as a carrier.
    ///
    /// 1. In practice we work with vectors of dimensionality 100 and each component has value in range [-1; 1]
    ///    There certainly is a possibility of underflow.
    ///    But we assume that such cases are rare and we can rely on such underflow losses.
    ///
    /// 2. According to the article http://www.ti3.tuhh.de/paper/rump/JeaRu13.pdf
    ///    the floating point rounding error is less then 100 * 2^-24 * sqrt(100) * sqrt(100) &lt; 0.0005960
    ///    We deem such precision is satisfactory for out needs.
    /// </remarks>
    public static class CosineDistance
    {
        /// <summary>
        /// Calculates cosine distance without making any optimizations.
        /// </summary>
        /// <param name="u">Left vector.</param>
        /// <param name="v">Right vector.</param>
        /// <returns>Cosine distance between u and v.</returns>
        public static float NonOptimized(IReadOnlyList<float> u, IReadOnlyList<float> v)
        {
            if (u.Count != v.Count)
            {
                throw new ArgumentException("Vectors have non-matching dimensions");
            }

            float dot = 0.0f;
            float nru = 0.0f;
            float nrv = 0.0f;
            for (int i = 0; i < u.Count; ++i)
            {
                dot += u[i] * v[i];
                nru += u[i] * u[i];
                nrv += v[i] * v[i];
            }

            var similarity = dot / (float)(Math.Sqrt(nru) * Math.Sqrt(nrv));
            return 1 - similarity;
        }

        /// <summary>
        /// Calculates cosine distance with assumption that u and v are unit vectors.
        /// </summary>
        /// <param name="u">Left vector.</param>
        /// <param name="v">Right vector.</param>
        /// <returns>Cosine distance between u and v.</returns>
        public static float ForUnits(IReadOnlyList<float> u, IReadOnlyList<float> v)
        {
            if (u.Count != v.Count)
            {
                throw new ArgumentException("Vectors have non-matching dimensions");
            }

            float dot = 0;
            for (int i = 0; i < u.Count; ++i)
            {
                dot += u[i] * v[i];
            }

            return 1 - dot;
        }

        /// <summary>
        /// Calculates cosine distance optimized using SIMD instructions.
        /// </summary>
        /// <param name="u">Left vector.</param>
        /// <param name="v">Right vector.</param>
        /// <returns>Cosine distance between u and v.</returns>
        public static float SIMD(float[] u, float[] v)
        {
            if (!Vector.IsHardwareAccelerated)
            {
                throw new NotSupportedException($"SIMD version of {nameof(CosineDistance)} is not supported");
            }

            if (u.Length != v.Length)
            {
                throw new ArgumentException("Vectors have non-matching dimensions");
            }

            float dot = 0;
            var norm = default(Vector2);
            int step = Vector<float>.Count;

            int i, to = u.Length - step;
            for (i = 0; i <= to; i += step)
            {
                var ui = new Vector<float>(u, i);
                var vi = new Vector<float>(v, i);
                dot += Vector.Dot(ui, vi);
                norm.X += Vector.Dot(ui, ui);
                norm.Y += Vector.Dot(vi, vi);
            }

            for (; i < u.Length; ++i)
            {
                dot += u[i] * v[i];
                norm.X += u[i] * u[i];
                norm.Y += v[i] * v[i];
            }

            norm = Vector2.SquareRoot(norm);
            var similarity = dot / (norm.X * norm.Y);
            return 1 - similarity;
        }

        /// <summary>
        /// Calculates cosine distance with assumption that u and v are unit vectors using SIMD instructions.
        /// </summary>
        /// <param name="u">Left vector.</param>
        /// <param name="v">Right vector.</param>
        /// <returns>Cosine distance between u and v.</returns>
        public static float SIMDForUnits(float[] u, float[] v)
        {
            if (!Vector.IsHardwareAccelerated)
            {
                throw new NotSupportedException($"SIMD version of {nameof(CosineDistance)} is not supported");
            }

            if (u.Length != v.Length)
            {
                throw new ArgumentException("Vectors have non-matching dimensions");
            }

            float dot = 0;
            int step = Vector<float>.Count;

            int i, to = u.Length - step;
            for (i = 0; i <= to; i += step)
            {
                var ui = new Vector<float>(u, i);
                var vi = new Vector<float>(v, i);
                dot += Vector.Dot(ui, vi);
            }

            for (; i < u.Length; ++i)
            {
                dot += u[i] * v[i];
            }

            return 1 - dot;
        }
    }
}
