using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RawGitLab.Models;

[JsonSerializable(typeof(StatusResponse))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
