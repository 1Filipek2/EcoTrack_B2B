using EcoTrack.Application.Features.Emissions.Commands.CreateEmission;
using EcoTrack.Application.Features.Emissions.Queries.GetCompanyEmissions;
using MediatR;

namespace EcoTrack.WebApi.Endpoints;

public static class EmissionEndpoints
{
    public static void MapEmissionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/emissions",
            async (CreateEmissionCommand command, IMediator mediator) =>
            {
                var id = await mediator.Send(command);
                return Results.Created($"/emission/{id}", id);
            });
        app.MapGet("/companies/{companyId:}/report",
            async (Guid companyId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCompanyEmissionQuery(companyId));

                return Results.Ok(result);
            });
    }    
}