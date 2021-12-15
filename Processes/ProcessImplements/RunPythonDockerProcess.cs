using System.Threading.Tasks;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes.ProcessImplements
{
    public class RunPythonDockerProcess : DockerProcess, IRunPythonProcess
    {
        public RunPythonDockerProcess()
        {
            Image = "python:3.8-alpine";
        }

        public async Task<int> Run(string code)
        {
            Cmd = new[] { "python3", "-u", "-c", code };
            return await Run();
        }
    }
}