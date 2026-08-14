using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                // Escape double quotes to prevent string injection/breaking shell arguments
                string escapedCommand = command.Replace("\"", "\\\"");

                string fileName;
                string arguments;

                if (isWindows)
                {
                    fileName = "powershell.exe";
                    arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{escapedCommand}\"";
                }
                else
                {
                    fileName = "/bin/bash";
                    arguments = $"-c \"{escapedCommand}\"";
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
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