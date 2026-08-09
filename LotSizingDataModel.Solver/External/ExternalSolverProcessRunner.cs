using System.Diagnostics;

namespace LotSizingDataModel.Solver.External;

/// <summary>
/// Executes one native solver command-line process with redirected output and
/// cooperative cancellation.
/// </summary>
public sealed class ExternalSolverProcessRunner
{
    private readonly object _syncRoot =
        new();

    private Process? _activeProcess;

    /// <summary>
    /// Executes a process asynchronously.
    /// </summary>
    /// <param name="executablePath">Executable path.</param>
    /// <param name="arguments">Individual command-line arguments.</param>
    /// <param name="workingDirectory">Process working directory.</param>
    /// <param name="standardInput">
    /// Optional text written to standard input after process start.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Captured process result.</returns>
    public async Task<ExternalSolverProcessResult> RunAsync(
        string executablePath,
        IEnumerable<string> arguments,
        string workingDirectory,
        string? standardInput,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            executablePath);
        ArgumentNullException.ThrowIfNull(
            arguments);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            workingDirectory);

        var startInfo =
            new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = standardInput is not null
            };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(
                argument);
        }

        using var process =
            new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

        if (!process.Start())
        {
            throw new InvalidOperationException(
                $"Unable to start solver executable '{executablePath}'.");
        }

        SetActiveProcess(
            process);

        int cancellationObserved =
            0;

        using CancellationTokenRegistration registration =
            cancellationToken.Register(
                () =>
                {
                    Interlocked.Exchange(
                        ref cancellationObserved,
                        1);

                    Stop();
                });

        try
        {
            Task<string> standardOutputTask =
                process.StandardOutput.ReadToEndAsync();

            Task<string> standardErrorTask =
                process.StandardError.ReadToEndAsync();

            if (standardInput is not null)
            {
                await process.StandardInput.WriteAsync(
                    standardInput);

                process.StandardInput.Close();
            }

            await process.WaitForExitAsync(
                CancellationToken.None);

            string standardOutput =
                await standardOutputTask;

            string standardError =
                await standardErrorTask;

            return new ExternalSolverProcessResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = standardOutput,
                StandardError = standardError,
                WasCancelled =
                    Volatile.Read(ref cancellationObserved) != 0 ||
                    cancellationToken.IsCancellationRequested
            };
        }
        finally
        {
            ClearActiveProcess(
                process);
        }
    }

    /// <summary>
    /// Requests termination of the currently active external process.
    /// </summary>
    public void Stop()
    {
        Process? process;

        lock (_syncRoot)
        {
            process =
                _activeProcess;
        }

        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(
                    entireProcessTree: true);
            }
        }
        catch
        {
            // Interruption is best effort. The solve path will report the
            // process outcome if native termination could not be requested.
        }
    }

    private void SetActiveProcess(
        Process process)
    {
        lock (_syncRoot)
        {
            _activeProcess =
                process;
        }
    }

    private void ClearActiveProcess(
        Process process)
    {
        lock (_syncRoot)
        {
            if (ReferenceEquals(
                    _activeProcess,
                    process))
            {
                _activeProcess =
                    null;
            }
        }
    }
}
