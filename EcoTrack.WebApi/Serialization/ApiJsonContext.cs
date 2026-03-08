using System.Text.Json.Serialization;
using EcoTrack.Application.DTOs;

namespace EcoTrack.WebApi.Serialization;

[JsonSerializable(typeof(EmissionDto))]
[JsonSerializable(typeof(List<EmissionDto>))]
public partial class ApiJsonContext : JsonSerializerContext
{
}