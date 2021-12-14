using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using AlgorithmEasy.Server.ProgramExecutionCenter.Hubs;
using AlgorithmEasy.Server.ProgramExecutionCenter.Processes.ProcessImplements;
using AlgorithmEasy.Shared.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlgorithmEasy.Server.ProgramExecutionCenter.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProgramExecuteController : ControllerBase
    {
        private readonly IHubContext<PythonExecuteHub> _pythonHubContext;

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public ProgramExecuteController(IHubContext<PythonExecuteHub> pythonHubContext) => _pythonHubContext = pythonHubContext;

        [HttpPost]
        public async Task<IActionResult> ExecutePython([Required] [FromBody] ExecutePythonRequest request)
        {
            var process = new RunPythonDockerProcess();
            process.OutputDataReceived += SendOutput;
            process.ErrorDataReceived += SendError;
            process.Exited += SendExit;

            var exitCode = await process.Run(request.Code);
            return Ok(exitCode);
        }

#nullable enable
        private async void SendOutput(object? sender, string? output)
        {
            if (output == null) return;
            try
            {
                await _pythonHubContext.Clients.User(UserId)
                    .SendAsync("ReceiveOutput", $"{output}\n");
            }
            catch
            {
                ((Process?)sender)?.Kill();
            }
        }

        private async void SendError(object? sender, string? error)
        {
            if (error == null) return;
            try
            {
                await _pythonHubContext.Clients.User(UserId)
                    .SendAsync("ReceiveOutput", $"<span class=\"text-red\">{error}</span>\n");
            }
            catch
            {
                ((Process?)sender)?.Kill();
            }
        }

        private async void SendExit(object? sender, int code)
        {
            try
            {
                await _pythonHubContext.Clients.User(UserId)
                    .SendAsync("ReceiveOutput", $"\nProcess finished with exit code {code}.");
            }
            catch
            {
                // ignored
            }
        }
#nullable disable
    }
}