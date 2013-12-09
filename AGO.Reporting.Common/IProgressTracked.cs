using System;

namespace AGO.Reporting.Common
{
    public interface IProgressTracker
    {
        byte PercentCompleted { get; }

        event EventHandler ProgressChanged;
    }
}