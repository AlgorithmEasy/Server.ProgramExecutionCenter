using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes.ProcessImplements
{
    public abstract class DockerProcess : IProcess
    {
        private CommonProcess _process;

        protected string Image { get; set; }
        protected IEnumerable<string> Cmd { get; set; }

#nullable enable
        public event EventHandler<string?>? OutputDataReceived;
        public event EventHandler<string?>? ErrorDataReceived;
#nullable disable
        public event EventHandler<int> Exited;

        public async Task<int> Run()
        {
            if (_process != null) return -1;

            var args = new List<string>(new[] { "run", "--rm", Image });
            args.AddRange(Cmd);

            _process = new()
            {
                Program = "docker",
                Args = args
            };

            _process.OutputDataReceived += (sender, s) => OutputDataReceived?.Invoke(sender, s);
            _process.ErrorDataReceived += (sender, s) => ErrorDataReceived?.Invoke(sender, s);
            _process.Exited += (sender, i) => Exited?.Invoke(sender, i);

            return await _process.Run();
        }

        public async Task<int> Run(string image, IEnumerable<string> cmd)
        {
            Image = image;
            Cmd = cmd;
            return await Run();
        }
    }
}