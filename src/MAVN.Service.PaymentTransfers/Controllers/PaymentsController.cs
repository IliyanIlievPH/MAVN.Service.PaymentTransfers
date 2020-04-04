using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using MAVN.Service.PaymentTransfers.Client;
using MAVN.Service.PaymentTransfers.Client.Enums;
using MAVN.Service.PaymentTransfers.Client.Models.Requests;
using MAVN.Service.PaymentTransfers.Client.Models.Responses;
using MAVN.Service.PaymentTransfers.Domain.Models;
using MAVN.Service.PaymentTransfers.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.PaymentTransfers.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase, IPaymentTransfersApi
    {
        private readonly IPaymentsService _paymentsService;
        private readonly IMapper _mapper;

        public PaymentsController(
            IPaymentsService paymentsService,
            IMapper mapper)
        {
            _paymentsService = paymentsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Transfer which is used when a customer wants to spend tokens on a campaign
        /// </summary>
        /// <returns><see cref="PaymentTransferResponse"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentTransferResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<PaymentTransferResponse> PaymentTransferAsync([FromBody] PaymentTransferRequest transferRequest)
        {
            var paymentTransferDto = _mapper.Map<PaymentTransferDto>(transferRequest);

            var result = await _paymentsService.PaymentTransferAsync(paymentTransferDto);

            return new PaymentTransferResponse
            {
                Error = (PaymentTransfersErrorCodes)result
            };
        }
    }
}
