using System.Data.Services;
using System.Data.Services.Common;
using DataServicesJSONP;

namespace FormProcessor
{
		[JSONPSupportBehavior]
    public class lookup : DataService<FormProcessorDbContext>
    {
        // This method is called only once to initialize service-wide policies.
        public static void InitializeService(DataServiceConfiguration config)
        {
            // TODO: set rules to indicate which entity sets and service operations are visible, updatable, etc.
            // Examples:
            // config.SetEntitySetAccessRule("MyEntityset", EntitySetRights.AllRead);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
						config.SetEntitySetAccessRule("Forms", EntitySetRights.AllRead);

        		config.UseVerboseErrors = true;
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }
    }
}
