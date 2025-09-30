using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

// ensure JIT and inlining differences minimized by single-thread measurement
long before = GC.GetAllocatedBytesForCurrentThread();

if (GC.TryStartNoGCRegion(before + 10_000))
{
    try
    {
        Person original = new("Markus", 42, "Developer", true, 55.112);
        long afterOriginal = GC.GetAllocatedBytesForCurrentThread();
        Person p2 = original with { Age = 43 };
        Consume(p2);
        long afterCopy = GC.GetAllocatedBytesForCurrentThread();
        long originalAllocation = checked(afterOriginal - before);
        long copyAllocation = checked(afterCopy - afterOriginal);
        Console.WriteLine($"Original allocation: {originalAllocation:N0} - Copy allocation: {copyAllocation:N0} bytes;");
    }
    finally
    {
        GC.EndNoGCRegion();
    }
}
else
{
    Debug.WriteLine("Could not enter NoGCRegion");
}

// prevent the optimizer from removing allocations
[MethodImpl(MethodImplOptions.NoInlining)]
static void Consume(object o)
{
    if (o == null)
    {
        Console.WriteLine("null");
    }
}

public record Person(string Name, int Age, string Occupation, bool Derpy, double Temp);