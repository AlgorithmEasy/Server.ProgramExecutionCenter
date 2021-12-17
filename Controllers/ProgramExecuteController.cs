using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using AlgorithmEasy.Server.ProgramExecutionCenter.Hubs;
using AlgorithmEasy.Server.ProgramExecutionCenter.Processes.ProcessImplements;
using AlgorithmEasy.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class ProgramExecuteController : ControllerBase
    {
        private readonly IHubContext<PythonExecuteHub> _pythonHubContext;

        public ProgramExecuteController(IHubContext<PythonExecuteHub> pythonHubContext) => _pythonHubContext = pythonHubContext;

        [HttpPost]
        public async Task<IActionResult> ExecutePython([Required] [FromBody] ExecutePythonRequest request)
        {
            var process = new RunPythonDockerProcess();
            process.OutputDataReceived += async (sender, output) =>
            {
                if (output == null) return;
                try
                {
                    await _pythonHubContext.Clients.Client(request.ConnectId)
                        .SendAsync("ReceiveOutput", $"{output}\n");
                }
                catch
                {
                    ((Process?)sender)?.Kill();
                }
            };
            process.ErrorDataReceived += async (sender, error) =>
            {
                if (error == null) return;
                try
                {
                    await _pythonHubContext.Clients.Client(request.ConnectId)
                        .SendAsync("ReceiveOutput", $"<span class=\"text-danger\">{error}</span>\n");
                }
                catch
                {
                    ((Process?)sender)?.Kill();
                }
            };
            process.Exited += async (sender, code) =>
            {
                try
                {
                    await _pythonHubContext.Clients.Client(request.ConnectId)
                        .SendAsync("ReceiveOutput", $"\nProcess finished with exit code {code}.");
                }
                catch
                {
                    // ignored
                }
            };

            var exitCode = process.Run(request.Code);
            return Ok(exitCode);
        }
    }
}