using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public virtual int Run()
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

            var exit = _process.WaitForExit(60 * 1000);
            if (!exit) _process.Kill();
            return _process.ExitCode;
        }

        public int Run(IEnumerable<string> args)
        {
            Args = args;
            return Run();
        }

        public void Stop() => _process.Kill();
    }
}