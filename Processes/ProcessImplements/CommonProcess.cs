using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Processes.ProcessImplements
{
    public class CommonProcess : IProcess
    {
        private Process _process;

        public string Program { get; init; }
        public IEnumerable<string> Args { get; set; }

#nullable enable
        public event EventHandler<string?>? OutputDataReceived;
        public event EventHandler<string?>? ErrorDataReceived;
#nullable disable
        public event EventHandler<int> Exited;

        public virtual async Task<int> Run()
        {
            if (_process != null || string.IsNullOrEmpty(Program)) return -1;

            _process = new()
            {
                StartInfo = new()
                {
                    FileName = Program,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            foreach (var arg in Args)
                _process.StartInfo.ArgumentList.Add(arg);

            _process.OutputDataReceived += (sender, args) => OutputDataReceived?.Invoke(sender, args.Data);
            _process.ErrorDataReceived += (sender, args) => ErrorDataReceived?.Invoke(sender, args.Data);
            _process.Exited += (_, _) => Exited?.Invoke(this, _process.ExitCode);

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await _process.WaitForExitAsync();
            return _process.ExitCode;
        }

        public async Task<int> Run(IEnumerable<string> args)
        {
            Args = args;
            return await Run();
        }

        public void Stop() => _process.Kill();
    }
}