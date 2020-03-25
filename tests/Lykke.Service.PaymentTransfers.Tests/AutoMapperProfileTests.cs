using AutoMapper;
using Lykke.Service.PaymentTransfers.MappingProfiles;
using Xunit;

namespace Lykke.Service.PaymentTransfers.Tests
{
    public class AutoMapperProfileTests
    {
        [Fact]
        public void Mapping_Configuration_Is_Correct()
        {
            // arrange

            var mockMapper = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });
            var mapper = mockMapper.CreateMapper();

            // act

            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // assert

            Assert.True(true);
        }
    }
}
