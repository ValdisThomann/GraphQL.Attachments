﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.Attachments
{
    public interface IIncomingAttachments:
        IReadOnlyDictionary<string, AttachmentStream>,
        IAsyncDisposable
    {
        AttachmentStream GetValue();
        bool TryGetValue([NotNullWhen(true)] out AttachmentStream? func);
    }
}