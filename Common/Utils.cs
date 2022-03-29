using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;

namespace Common
{
    public class Utils
    {
        public static Uri GetProxyAddress(Uri serviceName)
        {
            return new Uri($"http://localhost:19081{serviceName.AbsolutePath}");
        }

        public static Uri GetUsersServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/Users");
        }

        public static Uri GetRequestsServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/Requests");
        }

        public static Uri GetParcelsServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/Parcels");
        }

        // TODO: create partitioning scheme
        private long GetUsersPartitionKey(string name)
        {
            return 0;
        }

        private long GetRequestsPartitionKey(string name)
        {
            return 0;
        }

        private long GetParcelsPartitionKey(string name)
        {
            return 0;
        }
    }
}
