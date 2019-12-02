<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="https://raw.githubusercontent.com/SimonCropp/GraphQL.Attachments/master/src/icon.png" height="40px"> GraphQL.Attachments

[![Build status](https://ci.appveyor.com/api/projects/status/wq5ox06crbl9c2py/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/GraphQL.Attachments)
[![NuGet Status](https://img.shields.io/nuget/v/GraphQL.Attachments.svg?cacheSeconds=86400)](https://www.nuget.org/packages/GraphQL.Attachments/)

Provides access to a HTTP stream (via JavaScript on a web page) in [GraphQL](https://graphql-dotnet.github.io/) [Mutations](https://graphql-dotnet.github.io/docs/getting-started/mutations/) or [Queries](https://graphql-dotnet.github.io/docs/getting-started/queries). Attachments are transfered via a [multipart form](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition).


<!-- toc -->
## Contents

  * [Usage in Graphs](#usage-in-graphs)
  * [Server-side Controller](#server-side-controller)
    * [Non Attachments scenario](#non-attachments-scenario)
    * [With Attachments scenario](#with-attachments-scenario)
  * [Client - JavaScript](#client---javascript)
    * [Form submission](#form-submission)
  * [Client - .NET](#client---net)<!-- endtoc -->


## NuGet

https://nuget.org/packages/GraphQL.Attachments/

    PM> Install-Package GraphQL.Attachments


## Usage in Graphs

Incoming and Outgoing attachments can be accessed via the `ResolveFieldContext`:

<!-- snippet: UsageInGraphs -->
<a id='snippet-usageingraphs'/></a>
```cs
Field<ResultGraph>(
    "withAttachment",
    arguments: new QueryArguments(
        new QueryArgument<NonNullGraphType<StringGraphType>>
        {
            Name = "argument"
        }
    ),
    resolve: context =>
    {
        var incomingAttachments = context.IncomingAttachments();
        var outgoingAttachments = context.OutgoingAttachments();
        foreach (var incoming in incomingAttachments.Values)
        {
            using var memoryStream = new MemoryStream();
            incoming.CopyTo(memoryStream);
            outgoingAttachments.AddBytes(incoming.Name, memoryStream.ToArray());
        }

        return new Result
        {
            Argument = context.GetArgument<string>("argument"),
        };
    });
```
<sup>[snippet source](/src/Shared/Graphs/BaseRootGraph.cs#L24-L50) / [anchor](#snippet-usageingraphs)</sup>
<!-- endsnippet -->


## Server-side Controller


### Non Attachments scenario

In the usual usage scenario of graphQL in a Controller the query is passed in via a model object:

```cs
public class PostBody
{
    public string? OperationName;
    public string Query = null!;
    public JObject? Variables;
}
```

Which is then extracted using model binding:

```cs
[HttpPost]
public Task<ExecutionResult> Post(
    [BindRequired, FromBody] PostBody body)
{
    // run graphQL query
```


### With Attachments scenario


#### RequestReader instead of binding

When using Attachments the incoming request also requires the incoming form data to be parse. To facilitate this [RequestReader](/src/GraphQL.Attachments/RequestReader.cs) is used. This removes the requirement for model binding. The resulting Post and Get become:

<!-- snippet: ControllerPost -->
<a id='snippet-controllerpost'/></a>
```cs
[HttpPost]
public Task Post(CancellationToken cancellation)
{
    RequestReader.ReadPost(Request, out var query, out var inputs, out var incomingAttachments, out var operation);
    return Execute(query, operation, incomingAttachments, inputs, cancellation);
}
```
<sup>[snippet source](/src/SampleWeb/GraphQlController.cs#L24-L31) / [anchor](#snippet-controllerpost)</sup>
<!-- endsnippet -->

<!-- snippet: ControllerGet -->
<a id='snippet-controllerget'/></a>
```cs
[HttpGet]
public Task Get(CancellationToken cancellation)
{
    RequestReader.ReadGet(Request, out var query, out var inputs, out var operation);
    return Execute(query, operation, null, inputs,cancellation);
}
```
<sup>[snippet source](/src/SampleWeb/GraphQlController.cs#L33-L40) / [anchor](#snippet-controllerget)</sup>
<!-- endsnippet -->


#### Query Execution

To expose the attachments to the queries, the attachment context needs to be added to the `IDocumentExecuter`. This is done using `AttachmentsExtensions.ExecuteWithAttachments`:

<!-- snippet: ExecuteWithAttachments -->
<a id='snippet-executewithattachments'/></a>
```cs
var result = await executer.ExecuteWithAttachments(executionOptions, incomingAttachments);
```
<sup>[snippet source](/src/SampleWeb/GraphQlController.cs#L58-L60) / [anchor](#snippet-executewithattachments)</sup>
<!-- endsnippet -->


#### Result Writing

As with RequestReader for the incoming data, the outgoing data needs to be written with any resulting attachments. To facilitate this [ResponseWriter](/src/GraphQL.Attachments/ResponseWriter.cs) is used.

<!-- snippet: ResponseWriter -->
<a id='snippet-responsewriter'/></a>
```cs
await ResponseWriter.WriteResult(Response, result);
```
<sup>[snippet source](/src/SampleWeb/GraphQlController.cs#L61-L63) / [anchor](#snippet-responsewriter)</sup>
<!-- endsnippet -->


## Client - JavaScript


### Form submission

The JavaScript that submits the query does so through by building up a [FormData](https://developer.mozilla.org/en-US/docs/Web/API/FormData) object and [POSTing](https://developer.mozilla.org/en-US/docs/Learn/HTML/Forms/Sending_and_retrieving_form_data#The_POST_method) that via the [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API).

<!-- snippet: PostMutation -->
<a id='snippet-postmutation'/></a>
```html
function PostMutation() {
    var data = new FormData();
    var files = document.getElementById("files").files;
    for (var i = 0; i < files.length; i++) {
        data.append('files[]', files[i], files[i].name);
    }
    data.append("query", 'mutation{ withAttachment (argument: "argumentValue"){argument}}');

    var postSettings = {
        method: 'POST',
        body: data
    };

    return fetch('graphql', postSettings)
        .then(function (data) {
            return data.text().then(x => {
                result.innerHTML = x;
            });
        });
}
```
<sup>[snippet source](/src/SampleWeb/test.html#L5-L26) / [anchor](#snippet-postmutation)</sup>
<!-- endsnippet -->


## Client - .NET

Creating and posting a multipart form can be done using a combination of [MultipartFormDataContent](https://msdn.microsoft.com/en-us/library/system.net.http.multipartformdatacontent.aspx) and [HttpClient.PostAsync](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient.postasync.aspx). To simplify this action the `ClientQueryExecutor` class can be used:

<!-- snippet: ClientQueryExecutor.cs -->
<a id='snippet-ClientQueryExecutor.cs'/></a>
```cs
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.Attachments
{
    public class ClientQueryExecutor
    {
        HttpClient client;
        string uri;

        public ClientQueryExecutor(HttpClient client, string uri = "graphql")
        {
            Guard.AgainstNull(nameof(client), client);
            Guard.AgainstNullWhiteSpace(nameof(uri), uri);

            this.client = client;
            this.uri = uri;
        }

        public Task ExecutePost(string query, Action<Stream> resultAction, Action<Attachment>? attachmentAction = null, CancellationToken cancellation = default)
        {
            return ExecutePost(new PostRequest(query), resultAction, attachmentAction, cancellation);
        }

        public Task<QueryResult> ExecutePost(string query, CancellationToken cancellation = default)
        {
            return ExecutePost(new PostRequest(query), cancellation);
        }

        public async Task<QueryResult> ExecutePost(PostRequest request, CancellationToken cancellation = default)
        {
            var queryResult = new QueryResult();
            await ExecutePost(
                request,
                resultAction: stream => queryResult.ResultStream = stream,
                attachmentAction: attachment => queryResult.Attachments.Add(attachment.Name, attachment),
                cancellation);
            return queryResult;
        }

        public async Task ExecutePost(PostRequest request, Action<Stream> resultAction, Action<Attachment>? attachmentAction = null, CancellationToken cancellation = default)
        {
            Guard.AgainstNull(nameof(resultAction), resultAction);
            var content = new MultipartFormDataContent();
            content.AddQueryAndVariables(request.Query, request.Variables, request.OperationName);

            if (request.Action != null)
            {
                var postContext = new PostContext(content);
                request.Action?.Invoke(postContext);
                postContext.HeadersAction?.Invoke(content.Headers);
            }

            var response = await client.PostAsync(uri, content, cancellation);

            await response.ProcessResponse(resultAction, attachmentAction, cancellation);
        }

        public Task ExecuteGet(string query, Action<Stream> resultAction, Action<Attachment>? attachmentAction = null, CancellationToken cancellation = default)
        {
            return ExecuteGet(new GetRequest(query), resultAction, attachmentAction, cancellation);
        }

        public Task<QueryResult> ExecuteGet(string query, CancellationToken cancellation = default)
        {
            return ExecuteGet(new GetRequest(query), cancellation);
        }

        public async Task<QueryResult> ExecuteGet(GetRequest request, CancellationToken cancellation = default)
        {
            var queryResult = new QueryResult();
            await ExecuteGet(request,
                resultAction: stream => queryResult.ResultStream = stream,
                attachmentAction: attachment => queryResult.Attachments.Add(attachment.Name, attachment), cancellation);
            return queryResult;
        }

        public async Task ExecuteGet(GetRequest request, Action<Stream> resultAction, Action<Attachment>? attachmentAction = null, CancellationToken cancellation = default)
        {
            var compressed = Compress.Query(request.Query);
            var variablesString = GraphQlRequestAppender.ToJson(request.Variables);
            var getUri = UriBuilder.GetUri(uri, variablesString, compressed, request.OperationName);

            var getRequest = new HttpRequestMessage(HttpMethod.Get, getUri);
            request.HeadersAction?.Invoke(getRequest.Headers);
            var response = await client.SendAsync(getRequest, cancellation);
            await response.ProcessResponse(resultAction, attachmentAction, cancellation);
        }
    }
}
```
<sup>[snippet source](/src/GraphQL.Attachments.Client/ClientQueryExecutor.cs#L1-L93) / [anchor](#snippet-ClientQueryExecutor.cs)</sup>
<!-- endsnippet -->

This can be useful when performing [Integration testing in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/testing/integration-testing).


## Icon

<a href="https://thenounproject.com/term/database/1631008/" target="_blank">memory</a> designed by H Alberto Gongora from [The Noun Project](https://thenounproject.com)
