namespace SharedKernel
{
    using System.Threading;

    public static class ExecutionResume
    {
        private static int count;

        public static int Count
        {
            get { return count; }
        }

        public static void IncrementCount()
        {
            Interlocked.Increment(ref count);
        }
    }
}