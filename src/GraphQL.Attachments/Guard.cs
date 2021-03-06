using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

static class Guard
{
    public static void AgainstNull(string argumentName, object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNullWhiteSpace(string argumentName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegative(string argumentName, int value)
    {
        if (value < 0)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static Func<T> WrapFuncInCheck<T>(this Func<T> func, string name)
    {
        return () => func.EvaluateAndCheck(name);
    }

    static T EvaluateAndCheck<T>(this Func<T> func, string attachment)
    {
        var message = $"Provided delegate threw an exception. Attachment: {attachment}.";
        T value;
        try
        {
            value = func();
        }
        catch (Exception exception)
        {
            throw new Exception(message, exception);
        }

        ThrowIfNullReturned(null, attachment, value);
        return value;
    }

    static async Task<T> EvaluateAndCheck<T>(
        this Func<CancellationToken, Task<T>> func,
        string attachment,
        CancellationToken cancellation)
    {
        var message = $"Provided delegate threw an exception. Attachment: {attachment}.";
        T value;
        try
        {
            value = await func(cancellation);
        }
        catch (Exception exception)
        {
            throw new Exception(message, exception);
        }

        ThrowIfNullReturned(null, attachment, value);
        return value;
    }

    public static Action? WrapCleanupInCheck(this Action? cleanup, string attachment)
    {
        if (cleanup == null)
        {
            return null;
        }

        return () =>
        {
            try
            {
                cleanup();
            }
            catch (Exception exception)
            {
                throw new Exception($"Cleanup threw an exception. Attachment: {attachment}.", exception);
            }
        };
    }

    public static Func<CancellationToken, Task<T>> WrapFuncTaskInCheck<T>(
        this Func<CancellationToken, Task<T>> func,
        string attachment)
    {
        return async cancellation =>
        {
            var task = func.EvaluateAndCheck(attachment, cancellation);
            ThrowIfNullReturned(null, attachment, task);
            var value = await task;
            ThrowIfNullReturned(null, attachment, value);
            return value;
        };
    }

    public static Func<CancellationToken, Task<Stream>> WrapStreamFuncTaskInCheck<T>(
        this Func<CancellationToken, Task<T>> func,
        string attachment)
        where T : Stream
    {
        return async cancellation =>
        {
            var task = func.EvaluateAndCheck(attachment, cancellation);
            ThrowIfNullReturned(null, attachment, task);
            var value = await task;
            ThrowIfNullReturned(null, attachment, value);
            return value;
        };
    }

    public static void ThrowIfNullReturned(string? messageId, string attachment, object? value)
    {
        if (value != null)
        {
            return;
        }

        if (attachment != null && messageId != null)
        {
            throw new Exception($"Provided delegate returned a null. MessageId: '{messageId}', Attachment: '{attachment}'.");
        }

        if (attachment != null)
        {
            throw new Exception($"Provided delegate returned a null. Attachment: '{attachment}'.");
        }

        if (messageId != null)
        {
            throw new Exception($"Provided delegate returned a null. MessageId: '{messageId}'.");
        }

        throw new Exception("Provided delegate returned a null.");
    }
}