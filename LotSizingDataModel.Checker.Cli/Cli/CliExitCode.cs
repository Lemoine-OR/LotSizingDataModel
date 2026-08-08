namespace LotSizingDataModel.Checker.Cli;

internal enum CliExitCode
{
    Success = 0,
    ValidationFailed = 1,
    ExecutionFailure = 2,
    InvalidArguments = 3,
    Cancelled = 4,
    UnexpectedFailure = 5
}
