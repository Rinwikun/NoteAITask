using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NoteAITask.Services;

public class ShellExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class ShellService
{
    public async Task<ShellExecutionResult> ExecuteCommandAsync(string command)
    {
        return await Task.Run(() =>
        {
            var result = new ShellExecutionResult();
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe", // Otomatis fallback ke powershell di Windows
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    result.IsSuccess = true;
                    result.Output = output;
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = string.IsNullOrWhiteSpace(error)
                        ? $"Process exited with code {process.ExitCode}"
                        : error;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Gagal mengeksekusi perintah. Penyebab: {ex.Message}";
            }

            return result;
        });
    }
}