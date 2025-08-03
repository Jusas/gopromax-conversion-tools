namespace VideoConversionApp.Models;

public enum VideoRenderingState
{
    Queued,
    Rendering,
    CompletedSuccessfully,
    CompletedWithErrors,
    Canceled,
}