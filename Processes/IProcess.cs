using System;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes
{
    public interface IProcess
    {
        event EventHandler<string> OutputDataReceived;

        event EventHandler<string> ErrorDataReceived;

        event EventHandler<int> Exited;
    }
}