using Microsoft.Extensions.DependencyInjection;
using WorldPinball.Logistics.Application.Features.AllocateMachine;
using WorldPinball.Logistics.Application.Features.CancelShipment;
using WorldPinball.Logistics.Application.Features.CreateShipment;
using WorldPinball.Logistics.Application.Features.CreateShipment.Commands;
using WorldPinball.Logistics.Application.Features.DeliverShipment;
using WorldPinball.Logistics.Application.Features.DispatchShipment;
using WorldPinball.Logistics.Application.Features.GetShipment;

namespace WorldPinball.Logistics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLogisticsApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateShipmentHandler, CreateShipmentHandler>();
        services.AddScoped<IAllocateMachineHandler, AllocateMachineHandler>();
        services.AddScoped<IDispatchShipmentHandler, DispatchShipmentHandler>();
        services.AddScoped<IDeliverShipmentHandler, DeliverShipmentHandler>();
        services.AddScoped<ICancelShipmentHandler, CancelShipmentHandler>();
        services.AddScoped<IGetShipmentByIdHandler, GetShipmentByIdHandler>();
        
        return services;
    }
}
