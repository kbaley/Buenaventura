using AutoMapper;
using Buenaventura.Data;
using Buenaventura.Domain;
using Buenaventura.Shared;

namespace Buenaventura.Dtos;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Investment, InvestmentModel>()
            .ForMember(i => i.AveragePrice, opt => opt.MapFrom(src => Math.Round(src.GetAveragePricePaid(), 2)))
            .ForMember(i => i.Shares, opt => opt.MapFrom(src => src.GetNumberOfShares()))
            .ForMember(i => i.CurrentValue, opt => opt.MapFrom(src => Math.Round(src.GetCurrentValue(), 2)))
            .ReverseMap();  
        CreateMap<Investment, InvestmentForUpdateDto>().ReverseMap();
        CreateMap<InvestmentTransaction, InvestmentTransactionDto>()
            .ForMember(i => i.SourceAccountName, opt => opt.MapFrom(src => src.Transaction.Account!.Name))
            .ReverseMap();
        CreateMap<InvoiceLineItem, InvoiceLineItemsForPosting>()
            .ReverseMap();
        CreateMap<InvestmentCategory, InvestmentCategoryForUpdate>()
            .ReverseMap();
        CreateMap<Invoice, InvoiceForPosting>()
            .ReverseMap()
            .ForMember(dest => dest.Customer, opt => opt.Ignore());
        CreateMap<Account, AccountForPosting>().ReverseMap();
        CreateMap<Investment, InvestmentDetailDto>()
            .ForMember(i => i.Dividends, opt => opt.Ignore())
            .ForMember(i => i.AveragePrice, opt => opt.MapFrom(src => Math.Round(src.GetAveragePricePaid(), 2)))
            .ForMember(i => i.TotalReturn, opt => opt.MapFrom(src => src.GetTotalReturn()))
            .ForMember(i => i.TotalAnnualizedReturn, opt => opt.MapFrom(src => src.GetAnnualizedIrr()))
            .ForMember(i => i.Shares, opt => opt.MapFrom(src => src.GetNumberOfShares()));

        // Useful for visualizing an AutoMapper mapping
        // var config = new MapperConfiguration(cfg => cfg
        // .CreateMap<InvoiceForPosting, Invoice>()
        // .ForMember(s => s.Customer, opt => opt.Ignore()));
        // var execPlan = config.BuildExecutionPlan(typeof(InvoiceForPosting), typeof(Invoice));
        // var readable = execPlan.ToReadableString();
        // System.Console.WriteLine(readable);
    }

}