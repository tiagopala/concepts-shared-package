using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;

namespace Concepts.Shared.Package.Registers
{
    public static class DynamoDbServicesRegister
    {
        public static void RegisterDynamoDbServices(this IServiceCollection services)
        {
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
        }
    }
}
