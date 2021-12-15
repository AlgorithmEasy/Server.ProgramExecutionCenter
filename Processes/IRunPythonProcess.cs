using System.Threading.Tasks;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes
{
    public interface IRunPythonProcess
    {
        int Run(string code);
    }
}