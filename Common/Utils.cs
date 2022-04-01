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

        public static Uri GetManagementAPIServiceName(ServiceContext context)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/ManagementAPI");
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

        public static long GetUsersPartitionKeyFromId(string id)
        {
            int partitionEntrance = Char.ToUpper(id[1]) - 'A';
            return partitionEntrance % 2;
        }

        public static long GetUsersPartitionKeyFromCity(string city)
        {
            int partitionEntrance = Char.ToUpper(city[0]) - 'A';
            return partitionEntrance % 2;
        }

        public static long GetRequestsPartitionKeyFromId(string id)
        {
            int partitionEntrance = Char.ToUpper(id[1]) - 'A';
            return partitionEntrance % 4;
        }

        public static long GetRequestsPartitionKeyFromCity(string FromCity)
        {
            int partitionEntrance = Char.ToUpper(FromCity[0]) - 'A';
            return partitionEntrance % 4;
        }

        public static long GetParcelsPartitionKeyFromId(string id)
        {
            int partitionEntrance = Char.ToUpper(id[1]) - 'A';
            return partitionEntrance % 4;
        }

        public static long GetParcelsPartitionKeyFromCity(string FromCity)
        {
            int partitionEntrance = Char.ToUpper(FromCity[0]) - 'A';
            return partitionEntrance % 4;
        }
    }
}
