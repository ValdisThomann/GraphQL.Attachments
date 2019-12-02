﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Attachments;

class IncomingAttachments :
    Dictionary<string, AttachmentStream>,
    IIncomingAttachments
{
    public IncomingAttachments()
    {
    }

    public IncomingAttachments(Dictionary<string, AttachmentStream> dictionary) :
        base(dictionary)
    {
        Guard.AgainstNull(nameof(dictionary), dictionary);
    }

    public AttachmentStream GetValue()
    {
        if (TryGetValue(out var stream))
        {
            return stream;
        }

        throw new Exception("Attachment not found.");
    }

    public bool TryGetValue([NotNullWhen(true)] out AttachmentStream? stream)
    {
        if (Count == 0)
        {
            stream = null;
            return false;
        }

        EnsureSingle();

        stream = Values.Single();

        return true;
    }

    void EnsureSingle()
    {
        if (Count != 1)
        {
            throw new Exception("Reading an attachment with no name is only supported when their is a single attachment.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var stream in Values)
        {
            await stream.DisposeAsync();
        }
    }
}