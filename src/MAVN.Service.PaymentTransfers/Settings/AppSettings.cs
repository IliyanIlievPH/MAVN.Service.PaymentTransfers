using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Campaign.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.EligibilityEngine.Client;
using Lykke.Service.MAVNPropertyIntegration.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.WalletManagement.Client;

namespace MAVN.Service.PaymentTransfers.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PaymentTransfersSettings PaymentTransfersService { get; set; }

        public PrivateBlockchainFacadeServiceClientSettings PrivateBlockchainFacadeService { get; set; }

        public MAVNPropertyIntegrationServiceClientSettings RealEstateIntegrationService { get; set; }

        public EligibilityEngineServiceClientSettings EligibilityEngineService { get; set; }

        public CustomerProfileServiceClientSettings CustomerProfileService { get; set; }

        public WalletManagementServiceClientSettings WalletManagementService { get; set; }

        public CampaignServiceClientSettings CampaignService { get; set; }

    }
}
