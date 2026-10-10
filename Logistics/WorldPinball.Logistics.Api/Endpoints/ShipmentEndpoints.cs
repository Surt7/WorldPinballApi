using Microsoft.AspNetCore.Mvc;
using WorldPinball.Logistics.Api.Contracts.Shipment;
using WorldPinball.Logistics.Application.Features.AllocateMachine;
using WorldPinball.Logistics.Application.Features.AllocateMachine.Commands;
using WorldPinball.Logistics.Application.Features.CancelShipment;
using WorldPinball.Logistics.Application.Features.CancelShipment.Commands;
using WorldPinball.Logistics.Application.Features.CreateShipment;
using WorldPinball.Logistics.Application.Features.CreateShipment.Commands;
using WorldPinball.Logistics.Application.Features.DeliverShipment;
using WorldPinball.Logistics.Application.Features.DeliverShipment.Commands;
using WorldPinball.Logistics.Application.Features.DispatchShipment;
using WorldPinball.Logistics.Application.Features.DispatchShipment.Commands;
using WorldPinball.Logistics.Application.Features.GetShipment;
using WorldPinball.Logistics.Application.Features.GetShipment.Queries;
using GetShipmentByIdResult = WorldPinball.Logistics.Application.Features.GetShipment.GetShipmentByIdResult;

namespace WorldPinball.Logistics.Api.Endpoints;

public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/shipments")
            .WithTags("Shipments");

        group.MapPost("", async (
            [FromBody] CreateShipmentCommand command,
            [FromServices] ICreateShipmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            
            return result switch
            {
                CreateShipmentResult.Success success => 
                    Results.Created($"/api/v1/shipments/{success.ShipmentId}", new { success.ShipmentId }),
                
                CreateShipmentResult.ShipmentAlreadyExists conflict => 
                    Results.Conflict(new
                    {
                        error = $"Shipment already exists for order '{conflict.OrderId}'"
                    }),
                
                CreateShipmentResult.InvalidOrderData invalid => 
                    Results.BadRequest(new { error = invalid.Reason }),
                
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        
        group.MapPost("{id:guid}/allocate", async (
            Guid id,
            [FromBody] AllocateMachineRequest req,
            [FromServices] IAllocateMachineHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new AllocateMachineCommand(id, req.MachineId), ct);
            
            return result switch
            {
                AllocateMachineResult.Success => Results.NoContent(),
                
                AllocateMachineResult.ShipmentNotFound notFound => 
                    Results.NotFound(new { error = $"Shipment '{notFound.ShipmentId}' not found" }),
                
                AllocateMachineResult.CannotAllocate cannot => 
                    Results.UnprocessableEntity(new { error = cannot.Reason }),
                
                AllocateMachineResult.InvalidMachineId => 
                    Results.BadRequest(new { error = "Machine ID must be a non-empty GUID." }),
                
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        
        group.MapPost("{id:guid}/dispatch", async (
            Guid id, 
            [FromBody] DispatchShipmentRequest req,
            [FromServices] IDispatchShipmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new DispatchShipmentCommand(id, req.TrackingNumber), ct);
            
            return result switch
            {
                DispatchShipmentResult.Success => Results.NoContent(),
                DispatchShipmentResult.ShipmentNotFound n => 
                    Results.NotFound(new { error = $"Shipment '{n.ShipmentId}' not found" }),
                
                DispatchShipmentResult.CannotDispatch c =>
                    Results.UnprocessableEntity(new { error = c.Reason }),
                
                DispatchShipmentResult.InvalidTrackingNumber => 
                    Results.BadRequest(new { error = "Tracking number cannot be empty." }),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        
        
        group.MapPost("{id:guid}/deliver", async (
            Guid id,
            [FromServices] IDeliverShipmentHandler handler
            , CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new DeliverShipmentCommand(id), ct);
            
            return result switch
            {
                DeliverShipmentResult.Success => Results.NoContent(),
                DeliverShipmentResult.ShipmentNotFound n => Results.NotFound(
                    new { error = $"Shipment '{n.ShipmentId}' not found" }),
                
                DeliverShipmentResult.CannotDeliver c => 
                    Results.UnprocessableEntity(new { error = c.Reason }),
                
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });

        group.MapPost("{id:guid}/cancel", async (
            Guid id,
            [FromBody] CancelShipmentRequest req,
            [FromServices] ICancelShipmentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CancelShipmentCommand(id,
                req.Reason), ct);

            return result switch
            {
                CancelShipmentResult.Success => Results.NoContent(),
                CancelShipmentResult.ShipmentNotFound n =>
                    Results.NotFound(new
                    {
                        error = $"Shipment " +
                                $"'{n.ShipmentId}' not found"
                    }),
                CancelShipmentResult.CannotCancel c => Results.UnprocessableEntity(
                    new { error = c.Reason }),

                CancelShipmentResult.InvalidCancellationReason =>
                    Results.BadRequest(new { error = "Cancellation reason cannot be empty." }),
                
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        
        
        group.MapGet("{id:guid}", async (
            Guid id,
            [FromServices] IGetShipmentByIdHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetShipmentByIdQuery(id), ct);
            return result switch
            {
                GetShipmentByIdResult.Success s => Results.Ok(new ShipmentResponse(
                    s.Shipment.ShipmentId,
                    s.Shipment.OrderId,
                    s.Shipment.DestinationAddress,
                    s.Shipment.Status,
                    s.Shipment.MachineId,
                    s.Shipment.TrackingNumber,
                    s.Shipment.CancellationReason)),
                GetShipmentByIdResult.NotFound n => Results.NotFound(new { error = $"Shipment '{n.ShipmentId}' not found" }),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
        });
        
        
        return app;
    }

    
    public sealed record AllocateMachineRequest(Guid MachineId);
    public sealed record DispatchShipmentRequest(string TrackingNumber);
    public sealed record CancelShipmentRequest(string Reason);
}