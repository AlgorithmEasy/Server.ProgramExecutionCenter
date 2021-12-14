using System.Threading.Tasks;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes
{
    public interface IRunPythonProcess
    {
        Task<int> Run(string code);
    }
}