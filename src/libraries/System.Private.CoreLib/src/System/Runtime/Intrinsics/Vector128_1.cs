﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Runtime.Intrinsics
{
    // We mark certain methods with AggressiveInlining to ensure that the JIT will
    // inline them. The JIT would otherwise not inline the method since it, at the
    // point it tries to determine inline profitability, currently cannot determine
    // that most of the code-paths will be optimized away as "dead code".
    //
    // We then manually inline cases (such as certain intrinsic code-paths) that
    // will generate code small enough to make the AggressiveInlining profitable. The
    // other cases (such as the software fallback) are placed in their own method.
    // This ensures we get good codegen for the "fast-path" and allows the JIT to
    // determine inline profitability of the other paths as it would normally.


    /// <summary>Represents a 128-bit vector of a specified numeric type that is suitable for low-level optimization of parallel algorithms.</summary>
    /// <typeparam name="T">The type of the elements in the vector.</typeparam>
    [Intrinsic]
    [DebuggerDisplay("{DisplayString,nq}")]
    [DebuggerTypeProxy(typeof(Vector128DebugView<>))]
    [StructLayout(LayoutKind.Sequential, Size = Vector128.Size)]
    public readonly unsafe struct Vector128<T> : ISimdVector<Vector128<T>, T>
    {
        internal readonly Vector64<T> _lower;
        internal readonly Vector64<T> _upper;

        /// <summary>Gets a new <see cref="Vector128{T}" /> with all bits set to 1.</summary>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> AllBitsSet
        {
            [Intrinsic]
            get => Vector128.Create(Scalar<T>.AllBitsSet);
        }

        /// <summary>Gets the number of <typeparamref name="T" /> that are in a <see cref="Vector128{T}" />.</summary>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public static int Count
        {
            [Intrinsic]
            get
            {
                ThrowHelper.ThrowForUnsupportedIntrinsicsVector128BaseType<T>();
                return Vector128.Size / sizeof(T);
            }
        }

        /// <summary>Gets a new <see cref="Vector128{T}" /> with the elements set to their index.</summary>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> Indices
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelper.ThrowForUnsupportedIntrinsicsVector128BaseType<T>();
                Unsafe.SkipInit(out Vector128<T> result);

                for (int i = 0; i < Count; i++)
                {
                    result.SetElementUnsafe(i, Scalar<T>.Convert(i));
                }

                return result;
            }
        }

        /// <summary>Gets <c>true</c> if <typeparamref name="T" /> is supported; otherwise, <c>false</c>.</summary>
        /// <returns><c>true</c> if <typeparamref name="T" /> is supported; otherwise, <c>false</c>.</returns>
        public static bool IsSupported
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (typeof(T) == typeof(byte))
                    || (typeof(T) == typeof(double))
                    || (typeof(T) == typeof(short))
                    || (typeof(T) == typeof(int))
                    || (typeof(T) == typeof(long))
                    || (typeof(T) == typeof(nint))
                    || (typeof(T) == typeof(sbyte))
                    || (typeof(T) == typeof(float))
                    || (typeof(T) == typeof(ushort))
                    || (typeof(T) == typeof(uint))
                    || (typeof(T) == typeof(ulong))
                    || (typeof(T) == typeof(nuint));
            }
        }

        /// <summary>Gets a new <see cref="Vector128{T}" /> with all elements initialized to one.</summary>
        /// <exception cref="NotSupportedException">The type of the current instance (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> One
        {
            [Intrinsic]
            get => Vector128.Create(Scalar<T>.One);
        }

        /// <summary>Gets a new <see cref="Vector128{T}" /> with all elements initialized to zero.</summary>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public static Vector128<T> Zero
        {
            [Intrinsic]
            get
            {
                ThrowHelper.ThrowForUnsupportedIntrinsicsVector128BaseType<T>();
                return default;
            }
        }

        internal string DisplayString => IsSupported ? ToString() : SR.NotSupported_Type;

        /// <summary>Gets the element at the specified index.</summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The value of the element at <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> was less than zero or greater than the number of elements.</exception>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public T this[int index] => this.GetElement(index);

        /// <summary>Adds two vectors to compute their sum.</summary>
        /// <param name="left">The vector to add with <paramref name="right" />.</param>
        /// <param name="right">The vector to add with <paramref name="left" />.</param>
        /// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator +(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower + right._lower,
                left._upper + right._upper
            );
        }

        /// <summary>Computes the bitwise-and of two vectors.</summary>
        /// <param name="left">The vector to bitwise-and with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-and with <paramref name="left" />.</param>
        /// <returns>The bitwise-and of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator &(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower & right._lower,
                left._upper & right._upper
            );
        }

        /// <summary>Computes the bitwise-or of two vectors.</summary>
        /// <param name="left">The vector to bitwise-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to bitwise-or with <paramref name="left" />.</param>
        /// <returns>The bitwise-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator |(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower | right._lower,
                left._upper | right._upper
            );
        }

        /// <summary>Divides two vectors to compute their quotient.</summary>
        /// <param name="left">The vector that will be divided by <paramref name="right" />.</param>
        /// <param name="right">The vector that will divide <paramref name="left" />.</param>
        /// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator /(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower / right._lower,
                left._upper / right._upper
            );
        }

        /// <summary>Divides a vector by a scalar to compute the per-element quotient.</summary>
        /// <param name="left">The vector that will be divided by <paramref name="right" />.</param>
        /// <param name="right">The scalar that will divide <paramref name="left" />.</param>
        /// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator /(Vector128<T> left, T right)
        {
            return Vector128.Create(
                left._lower / right,
                left._upper / right
            );
        }

        /// <summary>Compares two vectors to determine if all elements are equal.</summary>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if all elements in <paramref name="left" /> were equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector128<T> left, Vector128<T> right)
        {
            return (left._lower == right._lower)
                && (left._upper == right._upper);
        }

        /// <summary>Computes the exclusive-or of two vectors.</summary>
        /// <param name="left">The vector to exclusive-or with <paramref name="right" />.</param>
        /// <param name="right">The vector to exclusive-or with <paramref name="left" />.</param>
        /// <returns>The exclusive-or of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator ^(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower ^ right._lower,
                left._upper ^ right._upper
            );
        }

        /// <summary>Compares two vectors to determine if any elements are not equal.</summary>
        /// <param name="left">The vector to compare with <paramref name="right" />.</param>
        /// <param name="right">The vector to compare with <paramref name="left" />.</param>
        /// <returns><c>true</c> if any elements in <paramref name="left" /> was not equal to the corresponding element in <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static bool operator !=(Vector128<T> left, Vector128<T> right) => !(left == right);

        /// <summary>Shifts each element of a vector left by the specified amount.</summary>
        /// <param name="value">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted left by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator <<(Vector128<T> value, int shiftCount)
        {
            return Vector128.Create(
                value._lower << shiftCount,
                value._upper << shiftCount
            );
        }

        /// <summary>Multiplies two vectors to compute their element-wise product.</summary>
        /// <param name="left">The vector to multiply with <paramref name="right" />.</param>
        /// <param name="right">The vector to multiply with <paramref name="left" />.</param>
        /// <returns>The element-wise product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator *(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower * right._lower,
                left._upper * right._upper
            );
        }

        /// <summary>Multiplies a vector by a scalar to compute their product.</summary>
        /// <param name="left">The vector to multiply with <paramref name="right" />.</param>
        /// <param name="right">The scalar to multiply with <paramref name="left" />.</param>
        /// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator *(Vector128<T> left, T right)
        {
            return Vector128.Create(
                left._lower * right,
                left._upper * right
            );
        }

        /// <summary>Multiplies a vector by a scalar to compute their product.</summary>
        /// <param name="left">The scalar to multiply with <paramref name="right" />.</param>
        /// <param name="right">The vector to multiply with <paramref name="left" />.</param>
        /// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator *(T left, Vector128<T> right) => right * left;

        /// <summary>Computes the ones-complement of a vector.</summary>
        /// <param name="vector">The vector whose ones-complement is to be computed.</param>
        /// <returns>A vector whose elements are the ones-complement of the corresponding elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator ~(Vector128<T> vector)
        {
            return Vector128.Create(
                ~vector._lower,
                ~vector._upper
            );
        }

        /// <summary>Shifts (signed) each element of a vector right by the specified amount.</summary>
        /// <param name="value">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator >>(Vector128<T> value, int shiftCount)
        {
            return Vector128.Create(
                value._lower >> shiftCount,
                value._upper >> shiftCount
            );
        }

        /// <summary>Subtracts two vectors to compute their difference.</summary>
        /// <param name="left">The vector from which <paramref name="right" /> will be subtracted.</param>
        /// <param name="right">The vector to subtract from <paramref name="left" />.</param>
        /// <returns>The difference of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator -(Vector128<T> left, Vector128<T> right)
        {
            return Vector128.Create(
                left._lower - right._lower,
                left._upper - right._upper
            );
        }

        /// <summary>Computes the unary negation of a vector.</summary>
        /// <param name="vector">The vector to negate.</param>
        /// <returns>A vector whose elements are the unary negation of the corresponding elements in <paramref name="vector" />.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator -(Vector128<T> vector)
        {
            if (typeof(T) == typeof(float))
            {
                return vector ^ Vector128.Create(-0.0f).As<float, T>();
            }
            else if (typeof(T) == typeof(double))
            {
                return vector ^ Vector128.Create(-0.0).As<double, T>();
            }
            else
            {
                return Zero - vector;
            }
        }

        /// <summary>Returns a given vector unchanged.</summary>
        /// <param name="value">The vector.</param>
        /// <returns><paramref name="value" /></returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        [Intrinsic]
        public static Vector128<T> operator +(Vector128<T> value)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector128BaseType<T>();
            return value;
        }

        /// <summary>Shifts (unsigned) each element of a vector right by the specified amount.</summary>
        /// <param name="value">The vector whose elements are to be shifted.</param>
        /// <param name="shiftCount">The number of bits by which to shift each element.</param>
        /// <returns>A vector whose elements where shifted right by <paramref name="shiftCount" />.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> operator >>>(Vector128<T> value, int shiftCount)
        {
            return Vector128.Create(
                value._lower >>> shiftCount,
                value._upper >>> shiftCount
            );
        }

        /// <summary>Determines whether the specified object is equal to the current instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="obj" /> is a <see cref="Vector128{T}" /> and is equal to the current instance; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public override bool Equals([NotNullWhen(true)] object? obj) => (obj is Vector128<T> other) && Equals(other);

        // Account for floating-point equality around NaN
        // This is in a separate method so it can be optimized by the mono interpreter/jiterpreter
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsFloatingPoint(Vector128<T> lhs, Vector128<T> rhs)
        {
            Vector128<T> result = Vector128.Equals(lhs, rhs) | ~(Vector128.Equals(lhs, lhs) | Vector128.Equals(rhs, rhs));
            return result.AsInt32() == Vector128<int>.AllBitsSet;
        }

        /// <summary>Determines whether the specified <see cref="Vector128{T}" /> is equal to the current instance.</summary>
        /// <param name="other">The <see cref="Vector128{T}" /> to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="other" /> is equal to the current instance; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector128<T> other)
        {
            // This function needs to account for floating-point equality around NaN
            // and so must behave equivalently to the underlying float/double.Equals

            if (Vector128.IsHardwareAccelerated)
            {
                if ((typeof(T) == typeof(double)) || (typeof(T) == typeof(float)))
                {
                    return EqualsFloatingPoint(this, other);
                }
                else
                {
                    return this == other;
                }
            }
            else
            {
                return _lower.Equals(other._lower)
                    && _upper.Equals(other._upper);
            }
        }

        /// <summary>Gets the hash code for the instance.</summary>
        /// <returns>The hash code for the instance.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public override int GetHashCode()
        {
            HashCode hashCode = default;

            for (int i = 0; i < Count; i++)
            {
                T value = this.GetElementUnsafe(i);
                hashCode.Add(value);
            }

            return hashCode.ToHashCode();
        }

        /// <summary>Converts the current instance to an equivalent string representation.</summary>
        /// <returns>An equivalent string representation of the current instance.</returns>
        /// <exception cref="NotSupportedException">The type of the vector (<typeparamref name="T" />) is not supported.</exception>
        public override string ToString() => ToString("G", CultureInfo.InvariantCulture);

        private string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
        {
            ThrowHelper.ThrowForUnsupportedIntrinsicsVector128BaseType<T>();

            var sb = new ValueStringBuilder(stackalloc char[64]);
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            sb.Append('<');
            sb.Append(((IFormattable)this.GetElementUnsafe(0)).ToString(format, formatProvider));

            for (int i = 1; i < Count; i++)
            {
                sb.Append(separator);
                sb.Append(' ');
                sb.Append(((IFormattable)this.GetElementUnsafe(i)).ToString(format, formatProvider));
            }
            sb.Append('>');

            return sb.ToString();
        }

        //
        // ISimdVector
        //

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Alignment" />
        static int ISimdVector<Vector128<T>, T>.Alignment => Vector128.Alignment;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ElementCount" />
        static int ISimdVector<Vector128<T>, T>.ElementCount => Vector128<T>.Count;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsHardwareAccelerated" />
        static bool ISimdVector<Vector128<T>, T>.IsHardwareAccelerated
        {
            [Intrinsic]
            get => Vector128.IsHardwareAccelerated;
        }

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Abs(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Abs(Vector128<T> vector) => Vector128.Abs(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Add(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Add(Vector128<T> left, Vector128<T> right) => left + right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.All(TSelf, T)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.All(Vector128<T> vector, T value) => Vector128.All(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.AllWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.AllWhereAllBitsSet(Vector128<T> vector) => Vector128.AllWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.AndNot(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.AndNot(Vector128<T> left, Vector128<T> right) => Vector128.AndNot(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Any(TSelf, T)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.Any(Vector128<T> vector, T value) => Vector128.Any(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.AnyWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.AnyWhereAllBitsSet(Vector128<T> vector) => Vector128.AnyWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.BitwiseAnd(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.BitwiseAnd(Vector128<T> left, Vector128<T> right) => left & right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.BitwiseOr(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.BitwiseOr(Vector128<T> left, Vector128<T> right) => left | right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Ceiling(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Ceiling(Vector128<T> vector) => Vector128.Ceiling(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Clamp(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Clamp(Vector128<T> value, Vector128<T> min, Vector128<T> max) => Vector128.Clamp(value, min, max);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ClampNative(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.ClampNative(Vector128<T> value, Vector128<T> min, Vector128<T> max) => Vector128.ClampNative(value, min, max);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ConditionalSelect(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.ConditionalSelect(Vector128<T> condition, Vector128<T> left, Vector128<T> right) => Vector128.ConditionalSelect(condition, left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CopySign(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.CopySign(Vector128<T> value, Vector128<T> sign) => Vector128.CopySign(value, sign);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CopyTo(TSelf, T[])" />
        static void ISimdVector<Vector128<T>, T>.CopyTo(Vector128<T> vector, T[] destination) => vector.CopyTo(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CopyTo(TSelf, T[], int)" />
        static void ISimdVector<Vector128<T>, T>.CopyTo(Vector128<T> vector, T[] destination, int startIndex) => vector.CopyTo(destination, startIndex);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CopyTo(TSelf, Span{T})" />
        static void ISimdVector<Vector128<T>, T>.CopyTo(Vector128<T> vector, Span<T> destination) => vector.CopyTo(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Count(TSelf, T)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.Count(Vector128<T> vector, T value) => Vector128.Count(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CountWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.CountWhereAllBitsSet(Vector128<T> vector) => Vector128.CountWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Create(T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Create(T value) => Vector128.Create(value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Create(T[])" />
        static Vector128<T> ISimdVector<Vector128<T>, T>.Create(T[] values) => Vector128.Create(values);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Create(T[], int)" />
        static Vector128<T> ISimdVector<Vector128<T>, T>.Create(T[] values, int index) => Vector128.Create(values, index);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Create(ReadOnlySpan{T})" />
        static Vector128<T> ISimdVector<Vector128<T>, T>.Create(ReadOnlySpan<T> values) => Vector128.Create(values);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CreateScalar(T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.CreateScalar(T value) => Vector128.CreateScalar(value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.CreateScalarUnsafe(T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.CreateScalarUnsafe(T value) => Vector128.CreateScalarUnsafe(value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Divide(TSelf, T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Divide(Vector128<T> left, Vector128<T> right) => left / right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Divide(TSelf, T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Divide(Vector128<T> left, T right) => left / right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Dot(TSelf, TSelf)" />
        [Intrinsic]
        static T ISimdVector<Vector128<T>, T>.Dot(Vector128<T> left, Vector128<T> right) => Vector128.Dot(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Equals(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Equals(Vector128<T> left, Vector128<T> right) => Vector128.Equals(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.EqualsAll(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.EqualsAll(Vector128<T> left, Vector128<T> right) => left == right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.EqualsAny(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.EqualsAny(Vector128<T> left, Vector128<T> right) => Vector128.EqualsAny(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Floor(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Floor(Vector128<T> vector) => Vector128.Floor(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GetElement(TSelf, int)" />
        [Intrinsic]
        static T ISimdVector<Vector128<T>, T>.GetElement(Vector128<T> vector, int index) => vector.GetElement(index);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThan(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.GreaterThan(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThan(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThanAll(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.GreaterThanAll(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThanAll(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThanAny(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.GreaterThanAny(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThanAny(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThanOrEqual(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.GreaterThanOrEqual(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThanOrEqual(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThanOrEqualAll(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.GreaterThanOrEqualAll(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThanOrEqualAll(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.GreaterThanOrEqualAny(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.GreaterThanOrEqualAny(Vector128<T> left, Vector128<T> right) => Vector128.GreaterThanOrEqualAny(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IndexOf(TSelf, T)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.IndexOf(Vector128<T> vector, T value) => Vector128.IndexOf(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IndexOfWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.IndexOfWhereAllBitsSet(Vector128<T> vector) => Vector128.IndexOfWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsEvenInteger(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsEvenInteger(Vector128<T> vector) => Vector128.IsEvenInteger(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsFinite(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsFinite(Vector128<T> vector) => Vector128.IsFinite(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsInfinity(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsInfinity(Vector128<T> vector) => Vector128.IsInfinity(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsInteger(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsInteger(Vector128<T> vector) => Vector128.IsInteger(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNaN(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsNaN(Vector128<T> vector) => Vector128.IsNaN(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNegative(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsNegative(Vector128<T> vector) => Vector128.IsNegative(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNegativeInfinity(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsNegativeInfinity(Vector128<T> vector) => Vector128.IsNegativeInfinity(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsNormal(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsNormal(Vector128<T> vector) => Vector128.IsNormal(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsOddInteger(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsOddInteger(Vector128<T> vector) => Vector128.IsOddInteger(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsPositive(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsPositive(Vector128<T> vector) => Vector128.IsPositive(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsPositiveInfinity(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsPositiveInfinity(Vector128<T> vector) => Vector128.IsPositiveInfinity(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsSubnormal(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsSubnormal(Vector128<T> vector) => Vector128.IsSubnormal(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.IsZero(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.IsZero(Vector128<T> vector) => Vector128.IsZero(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LastIndexOf(TSelf, T)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.LastIndexOf(Vector128<T> vector, T value) => Vector128.LastIndexOf(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LastIndexOfWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static int ISimdVector<Vector128<T>, T>.LastIndexOfWhereAllBitsSet(Vector128<T> vector) => Vector128.LastIndexOfWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThan(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LessThan(Vector128<T> left, Vector128<T> right) => Vector128.LessThan(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThanAll(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.LessThanAll(Vector128<T> left, Vector128<T> right) => Vector128.LessThanAll(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThanAny(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.LessThanAny(Vector128<T> left, Vector128<T> right) => Vector128.LessThanAny(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThanOrEqual(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LessThanOrEqual(Vector128<T> left, Vector128<T> right) => Vector128.LessThanOrEqual(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThanOrEqualAll(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.LessThanOrEqualAll(Vector128<T> left, Vector128<T> right) => Vector128.LessThanOrEqualAll(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LessThanOrEqualAny(TSelf, TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.LessThanOrEqualAny(Vector128<T> left, Vector128<T> right) => Vector128.LessThanOrEqualAny(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Load(T*)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Load(T* source) => Vector128.Load(source);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LoadAligned(T*)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LoadAligned(T* source) => Vector128.LoadAligned(source);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LoadAlignedNonTemporal(T*)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LoadAlignedNonTemporal(T* source) => Vector128.LoadAlignedNonTemporal(source);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LoadUnsafe(ref readonly T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LoadUnsafe(ref readonly T source) => Vector128.LoadUnsafe(in source);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.LoadUnsafe(ref readonly T, nuint)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.LoadUnsafe(ref readonly T source, nuint elementOffset) => Vector128.LoadUnsafe(in source, elementOffset);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Max(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Max(Vector128<T> left, Vector128<T> right) => Vector128.Max(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxMagnitude(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MaxMagnitude(Vector128<T> left, Vector128<T> right) => Vector128.MaxMagnitude(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxMagnitudeNumber(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MaxMagnitudeNumber(Vector128<T> left, Vector128<T> right) => Vector128.MaxMagnitudeNumber(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxNative(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MaxNative(Vector128<T> left, Vector128<T> right) => Vector128.MaxNative(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MaxNumber(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MaxNumber(Vector128<T> left, Vector128<T> right) => Vector128.MaxNumber(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Min(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Min(Vector128<T> left, Vector128<T> right) => Vector128.Min(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinMagnitude(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MinMagnitude(Vector128<T> left, Vector128<T> right) => Vector128.MinMagnitude(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinMagnitudeNumber(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MinMagnitudeNumber(Vector128<T> left, Vector128<T> right) => Vector128.MinMagnitudeNumber(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinNative(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MinNative(Vector128<T> left, Vector128<T> right) => Vector128.MinNative(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MinNumber(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MinNumber(Vector128<T> left, Vector128<T> right) => Vector128.MinNumber(left, right);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Multiply(TSelf, T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Multiply(Vector128<T> left, Vector128<T> right) => left * right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Multiply(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Multiply(Vector128<T> left, T right) => left * right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.MultiplyAddEstimate(TSelf, TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.MultiplyAddEstimate(Vector128<T> left, Vector128<T> right, Vector128<T> addend) => Vector128.MultiplyAddEstimate(left, right, addend);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Negate(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Negate(Vector128<T> vector) => -vector;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.None(TSelf, T)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.None(Vector128<T> vector, T value) => Vector128.None(vector, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.NoneWhereAllBitsSet(TSelf)" />
        [Intrinsic]
        static bool ISimdVector<Vector128<T>, T>.NoneWhereAllBitsSet(Vector128<T> vector) => Vector128.NoneWhereAllBitsSet(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.OnesComplement(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.OnesComplement(Vector128<T> vector) => ~vector;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Round(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Round(Vector128<T> vector) => Vector128.Round(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ShiftLeft(TSelf, int)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.ShiftLeft(Vector128<T> vector, int shiftCount) => vector << shiftCount;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ShiftRightArithmetic(TSelf, int)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.ShiftRightArithmetic(Vector128<T> vector, int shiftCount) => vector >> shiftCount;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ShiftRightLogical(TSelf, int)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.ShiftRightLogical(Vector128<T> vector, int shiftCount) => vector >>> shiftCount;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Sqrt(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Sqrt(Vector128<T> vector) => Vector128.Sqrt(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Store(TSelf, T*)" />
        [Intrinsic]
        static void ISimdVector<Vector128<T>, T>.Store(Vector128<T> source, T* destination) => source.Store(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.StoreAligned(TSelf, T*)" />
        [Intrinsic]
        static void ISimdVector<Vector128<T>, T>.StoreAligned(Vector128<T> source, T* destination) => source.StoreAligned(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.StoreAlignedNonTemporal(TSelf, T*)" />
        [Intrinsic]
        static void ISimdVector<Vector128<T>, T>.StoreAlignedNonTemporal(Vector128<T> source, T* destination) => source.StoreAlignedNonTemporal(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.StoreUnsafe(TSelf, ref T)" />
        [Intrinsic]
        static void ISimdVector<Vector128<T>, T>.StoreUnsafe(Vector128<T> vector, ref T destination) => vector.StoreUnsafe(ref destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.StoreUnsafe(TSelf, ref T, nuint)" />
        [Intrinsic]
        static void ISimdVector<Vector128<T>, T>.StoreUnsafe(Vector128<T> vector, ref T destination, nuint elementOffset) => vector.StoreUnsafe(ref destination, elementOffset);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Subtract(TSelf, TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Subtract(Vector128<T> left, Vector128<T> right) => left - right;

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Sum(TSelf)" />
        [Intrinsic]
        static T ISimdVector<Vector128<T>, T>.Sum(Vector128<T> vector) => Vector128.Sum(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.ToScalar(TSelf)" />
        [Intrinsic]
        static T ISimdVector<Vector128<T>, T>.ToScalar(Vector128<T> vector) => vector.ToScalar();

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Truncate(TSelf)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Truncate(Vector128<T> vector) => Vector128.Truncate(vector);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.TryCopyTo(TSelf, Span{T})" />
        static bool ISimdVector<Vector128<T>, T>.TryCopyTo(Vector128<T> vector, Span<T> destination) => vector.TryCopyTo(destination);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.WithElement(TSelf, int, T)" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.WithElement(Vector128<T> vector, int index, T value) => vector.WithElement(index, value);

        /// <inheritdoc cref="ISimdVector{TSelf, T}.Xor" />
        [Intrinsic]
        static Vector128<T> ISimdVector<Vector128<T>, T>.Xor(Vector128<T> left, Vector128<T> right) => left ^ right;
    }
}
