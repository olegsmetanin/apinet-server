using System;

namespace AGO.Reporting.Common
{
    public interface IProgressTracker
    {
        int PercentCompleted { get; }

        event EventHandler ProgressChanged;
    }
}