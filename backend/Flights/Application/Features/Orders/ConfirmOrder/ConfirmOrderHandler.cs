using Flights.Domain.Interfaces;
using MediatR;

namespace Flights.Application.Features.Orders.ConfirmOrder;

public class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepo;

    public ConfirmOrderHandler(
        IUnitOfWork unitOfWork, 
        IOrderRepository orderRepo)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, cancellationToken);
        if(order is null)
            throw new ApplicationException("Order not found");
        
        order.Confirm(request.Email);
        await _unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}