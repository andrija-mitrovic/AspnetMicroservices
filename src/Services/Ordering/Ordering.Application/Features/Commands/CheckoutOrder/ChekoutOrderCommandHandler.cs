using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Contracts.Persistence;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Commands.CheckoutOrder
{
    public class ChekoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, int>
    {
        private readonly IOrderRepository _orderRespository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<ChekoutOrderCommandHandler> _logger;

        public ChekoutOrderCommandHandler(IOrderRepository orderRespository,
            IMapper mapper,
            IEmailService emailService,
            ILogger<ChekoutOrderCommandHandler> logger)
        {
            _orderRespository = orderRespository ?? throw new ArgumentNullException(nameof(orderRespository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
        {
            var orderEntity = _mapper.Map<Order>(request);
            var newOrder = await _orderRespository.AddAsync(orderEntity);

            _logger.LogInformation($"Order {newOrder.Id} is successfully created.");

            await SendEmail(newOrder);

            return newOrder.Id;
        }

        private async Task SendEmail(Order order)
        {
            var email = new Email()
            {
                To = "andrija@gmail.com",
                Body = $"Order was created.",
                Subject = "Order was created"
            };

            try
            {
                await _emailService.SendEmail(email);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Order {order.Id} failed due to an error with the mail service: {ex.Message}");
            }
        }
    }
}
