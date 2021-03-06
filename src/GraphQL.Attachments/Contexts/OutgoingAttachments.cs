﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Attachments;

class OutgoingAttachments :
    IOutgoingAttachments
{
    internal Dictionary<string, Outgoing> Inner = new Dictionary<string, Outgoing>(StringComparer.OrdinalIgnoreCase);

    public bool HasPendingAttachments => Inner.Any();

    public IReadOnlyList<string> Names => Inner.Keys.ToList();

    public void AddStream<T>(Func<CancellationToken, Task<T>> streamFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
        where T : Stream
    {
        AddStream("default", streamFactory, cleanup, headers);
    }

    public void AddStream<T>(string name, Func<CancellationToken, Task<T>> streamFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
        where T : Stream
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(streamFactory), streamFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: async cancellation =>
                {
                    streamFactory = streamFactory.WrapFuncTaskInCheck(name);
                    var value = await streamFactory(cancellation);
                    return new StreamContent(value);
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddStream(Func<Stream> streamFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddStream("default", streamFactory, cleanup, headers);
    }

    public void AddStream(Stream stream, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddStream("default", stream, cleanup, headers);
    }

    public void AddStream(string name, Func<Stream> streamFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(streamFactory), streamFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation =>
                {
                    streamFactory = streamFactory.WrapFuncInCheck(name);
                    var value = streamFactory();
                    return Task.FromResult<HttpContent>(new StreamContent(value));
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddStream(string name, Stream stream, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(stream), stream);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation => Task.FromResult<HttpContent>(new StreamContent(stream)),
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddBytes(Func<byte[]> bytesFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddBytes("default", bytesFactory, cleanup, headers);
    }

    public void AddBytes(byte[] bytes, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddBytes("default", bytes, cleanup, headers);
    }

    public void AddBytes(string name, Func<byte[]> bytesFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(bytesFactory), bytesFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation =>
                {
                    bytesFactory = bytesFactory.WrapFuncInCheck(name);
                    var value = bytesFactory();
                    return Task.FromResult<HttpContent>(new ByteArrayContent(value));
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddBytes(string name, byte[] bytes, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(bytes), bytes);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation => Task.FromResult<HttpContent>(new ByteArrayContent(bytes)),
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddBytes(Func<CancellationToken, Task<byte[]>> bytesFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddBytes("default", bytesFactory, cleanup, headers);
    }

    public void AddBytes(string name, Func<CancellationToken, Task<byte[]>> bytesFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(bytesFactory), bytesFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: async cancellation =>
                {
                    bytesFactory = bytesFactory.WrapFuncTaskInCheck(name);
                    var value = await bytesFactory(cancellation);
                    return new ByteArrayContent(value);
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddString(Func<string> valueFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddString("default", valueFactory, cleanup, headers);
    }

    public void AddString(string value, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddString("default", value, cleanup, headers);
    }

    public void AddString(string name, Func<string> valueFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(valueFactory), valueFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation =>
                {
                    valueFactory = valueFactory.WrapFuncInCheck(name);
                    var value = valueFactory();
                    return Task.FromResult<HttpContent>(new StringContent(value));
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddString(string name, string value, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(value), value);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: cancellation => Task.FromResult<HttpContent>(new StringContent(value)),
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }

    public void AddString(Func<CancellationToken, Task<string>> valueFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        AddString("default", valueFactory, cleanup, headers);
    }

    public void AddString(string name, Func<CancellationToken, Task<string>> valueFactory, Action? cleanup = null, HttpContentHeaders? headers = null)
    {
        Guard.AgainstNull(nameof(name), name);
        Guard.AgainstNull(nameof(valueFactory), valueFactory);
        Inner.Add(name,
            new Outgoing
            (
                contentBuilder: async cancellation =>
                {
                    valueFactory = valueFactory.WrapFuncTaskInCheck(name);
                    var value = await valueFactory(cancellation);
                    return new StringContent(value);
                },
                cleanup: cleanup.WrapCleanupInCheck(name),
                headers: headers
            ));
    }
}