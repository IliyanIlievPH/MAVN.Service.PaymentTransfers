using AutoMapper;
using Falcon.Numerics;
using Lykke.PrivateBlockchain.Definitions;
using MAVN.Service.PaymentTransfers.Client.Models.Requests;
using MAVN.Service.PaymentTransfers.Client.Models.Responses;
using MAVN.Service.PaymentTransfers.Domain.Models;

namespace MAVN.Service.PaymentTransfers.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TransferReceivedEventDTO, TransferReceivedModel>()
                .ForMember(p => p.Amount, opt => opt.MapFrom(src => Money18.CreateFromAtto(src.Amount)));
            CreateMap<TransferAcceptedEventDTO, TransferAcceptedModel>();
            CreateMap<TransferRejectedEventDTO, TransferRejectedModel>();
            CreateMap<PaymentTransferRequest, PaymentTransferDto>()
                .ForMember(p => p.InvoiceId, opt => opt.Ignore())
                .ForMember(p => p.ReceiptNumber, opt => opt.Ignore())
                .ForMember(p => p.Timestamp, opt => opt.Ignore())
                .ForMember(p => p.Status, opt => opt.Ignore())
                .ForMember(p => p.TransferId, opt => opt.Ignore());
        }
    }
}
