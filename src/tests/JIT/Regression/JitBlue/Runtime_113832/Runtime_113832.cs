// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Generated by Fuzzlyn v2.5 on 2025-03-23 15:25:27
// Run on X86 Windows
// Seed: 916448399438567841-vectort,vector128,vector256,x86aes,x86avx,x86avx2,x86avx512bw,x86avx512bwvl,x86avx512cd,x86avx512cdvl,x86avx512dq,x86avx512dqvl,x86avx512f,x86avx512fvl,x86bmi1,x86bmi2,x86fma,x86lzcnt,x86pclmulqdq,x86popcnt,x86sse,x86sse2,x86sse3,x86sse41,x86sse42,x86ssse3,x86x86base
// Reduced from 63.4 KiB to 0.7 KiB in 00:02:21
// Problem() Hits JIT assert in Release:
// Assertion failed '(consume == 0) || (ComputeAvailableSrcCount(tree) == consume)' in 'Program:Main(Fuzzlyn.ExecutionServer.IRuntime)' during 'Linear scan register alloc' (IL size 68; hash 0xade6b36b; FullOpts)

// Generated by Fuzzlyn v2.5 on 2025-03-27 16:58:00
// Run on X86 Windows
// Seed: 10696738320409793384-vectort,vector128,vector256,x86aes,x86avx,x86avx2,x86avx512bw,x86avx512bwvl,x86avx512cd,x86avx512cdvl,x86avx512dq,x86avx512dqvl,x86avx512f,x86avx512fvl,x86bmi1,x86bmi2,x86fma,x86lzcnt,x86pclmulqdq,x86popcnt,x86sse,x86sse2,x86sse3,x86sse41,x86sse42,x86ssse3,x86x86base
// Reduced from 151.7 KiB to 0.8 KiB in 00:02:43
// Problem2() Hits JIT assert in Release:
// Assertion failed 'hwintrinsicChild->isContained()' in 'Program:Main(Fuzzlyn.ExecutionServer.IRuntime)' during 'Generate code' (IL size 94; hash 0xade6b36b; FullOpts)
using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Xunit;

public class Runtime_113832
{
    public static byte s_4;

    [Fact]
    public static void Problem()
    {
        if (Avx512F.VL.IsSupported)
        {
            var vr9 = Vector128.Create<ulong>(0);
            var vr10 = (ulong)s_4;
            var vr11 = Vector128.CreateScalar(vr10);
            var vr12 = Avx2.BroadcastScalarToVector128(vr11);
            var vr13 = (byte)0;
            var vr14 = Vector256.CreateScalar(vr13);
            var vr15 = (ulong)Avx2.MoveMask(vr14);
            var vr16 = Vector128.Create<ulong>(vr15);
            var vr17 = Avx512F.VL.TernaryLogic(vr9, vr12, vr16, 1);
            Console.WriteLine(vr17);
        }
    }

    public static ulong s_26;

    [Fact]
    public static void Problem2()
    {
        if (Avx512F.VL.IsSupported)
        {
            Vector256<short> vr14 = default;
            var vr15 = s_26++;
            var vr16 = Vector128.CreateScalar(vr15);
            var vr17 = Avx2.BroadcastScalarToVector128(vr16);
            var vr18 = Vector128.Create<ulong>(0);
            var vr19 = Vector128.Create<ulong>(0);
            var vr20 = Sse2.ShiftRightLogical128BitLane(vr19, 0);
            var vr21 = Avx512F.VL.PermuteVar2x64x2(vr17, vr18, vr20);
            var vr22 = Vector128.Create<ulong>(0);
            if (Sse41.TestZ(vr21, vr22))
            {
                Console.WriteLine(vr14);
            }
        }
    }
}
