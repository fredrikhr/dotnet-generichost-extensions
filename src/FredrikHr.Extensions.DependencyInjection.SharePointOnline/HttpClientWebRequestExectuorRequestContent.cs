using System.Net;

namespace Microsoft.SharePoint.Client;

internal sealed class HttpClientWebRequestExectuorRequestContent() : HttpContent()
{
    internal TaskCompletionSource<Stream> StreamCompletionSource { get; }
        = new();
    internal TaskCompletionSource<object?> RequestPayloadCompletionSource { get; }
        = new();

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        bool streamSignalSuccess = StreamCompletionSource.TrySetResult(stream);
        if (!streamSignalSuccess)
            throw new InvalidOperationException();
        await RequestPayloadCompletionSource.Task
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            StreamCompletionSource.TrySetCanceled();
            RequestPayloadCompletionSource.TrySetCanceled();
        }
    }
}
