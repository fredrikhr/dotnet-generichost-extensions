namespace Microsoft.SharePoint.Client;

internal sealed class HttpClientWebRequestExectuorRequestStream(
    Stream httpRequestStream,
    TaskCompletionSource<object?> taskCompletion
    ) : Stream()
{
    private bool _disposed;
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => httpRequestStream.CanWrite;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        httpRequestStream.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return httpRequestStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? asyncCallback, object? asyncState)
    {
        ThrowIfDisposed();
        return httpRequestStream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        ThrowIfDisposed();
        httpRequestStream.EndWrite(asyncResult);
    }

    public override void Flush()
    {
        ThrowIfDisposed();
        httpRequestStream.Flush();
    }

    public void Complete()
    {
        taskCompletion.TrySetResult(default);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
        }
        httpRequestStream.Flush();
        Complete();
        base.Dispose(disposing);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return httpRequestStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
