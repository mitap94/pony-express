using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using System.Net.Http;
using System.Fabric.Description;
using Common;

namespace ManagementAPI
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class ManagementAPI : StatelessService
    {
        private static readonly object metricsLock = new object();

        private static readonly string TotalRequestsPerMinuteMetricName = "TotalRequestsPerMinute";
        private static readonly TimeSpan MetricsReportingInterval = TimeSpan.FromSeconds(30);
        private static int TotalNumRequestPrevious30s = 0;
        private static int TotalNumRequestsThese30s = 0;

        private static readonly string UserCreationPerMinuteMetricName = "UserCreationPerMinute";
        private static readonly TimeSpan LongerMetricsReportingInterval = TimeSpan.FromMinutes(1);
        private static int NumUserCreationsPerMinute = 0;

        private static readonly string ParcelRequestCreationPerMinuteMetricName = "ParcelRequestCreationPerMinute";
        private static int NumParcelRequestCreationPerMinute = 0;

        private static readonly string ParcelRequestApprovalPerMinuteMetricName = "ParcelRequestApprovalPerMinute";
        private static int NumParcelRequestApprovalPerMinute = 0;

        private static readonly string ParcelRequestDenialPerMinuteMetricName = "ParcelRequestDenialPerMinute";
        private static int NumParcelRequestDenialPerMinute = 0;

        private static readonly string ParcelPickupPerMinuteMetricName = "ParcelPickupPerMinute";
        private static int NumParcelPickupPerMinute = 0;

        private static readonly string ParcelDeliveryPerMinuteMetricName = "ParcelDeliveryPerMinute";
        private static int NumParcelDeliveryPerMinute = 0;


        private static readonly TimeSpan ScaleInterval = TimeSpan.FromMinutes(1);

        public ManagementAPI(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"ManagementAPI Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<HttpClient>(new HttpClient())
                                            .AddSingleton<FabricClient>(new FabricClient())
                                            .AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            DefineMetricsAndPolicies();

            // Report total number of requests faster - 30s
            _ = Task.Run(async () =>
              {
                  while (true)
                  {
                      cancellationToken.ThrowIfCancellationRequested();

                      // Reporting LoadMetrics
                      lock (metricsLock) {
                          ServiceEventSource.Current.ServiceMessage(this.Context, $"ManagementAPI {Context.InstanceId} report {TotalRequestsPerMinuteMetricName} : {TotalNumRequestPrevious30s + TotalNumRequestsThese30s}");
                          Partition.ReportLoad(new List<LoadMetric> { new LoadMetric(TotalRequestsPerMinuteMetricName, TotalNumRequestPrevious30s + TotalNumRequestsThese30s) });
                          TotalNumRequestPrevious30s = TotalNumRequestsThese30s;
                          TotalNumRequestsThese30s = 0;
                      }

                      await Task.Delay(MetricsReportingInterval, cancellationToken);
                  }
              },cancellationToken);

            // Report all other requests - 60s
            await Task.Run(async () =>
              {
                  while (true)
                  {
                      cancellationToken.ThrowIfCancellationRequested();

                      // Reporting LoadMetrics
                      lock (metricsLock)
                      {
                          ServiceEventSource.Current.ServiceMessage(this.Context, $"ManagementAPI {Context.InstanceId} report " +
                            $"{UserCreationPerMinuteMetricName} : {NumUserCreationsPerMinute},\n" +
                            $"{ParcelRequestCreationPerMinuteMetricName} : {NumParcelRequestCreationPerMinute},\n" +
                            $"{ParcelRequestApprovalPerMinuteMetricName} : {NumParcelRequestApprovalPerMinute},\n" +
                            $"{ParcelRequestDenialPerMinuteMetricName} : {NumParcelRequestDenialPerMinute},\n" +
                            $"{ParcelPickupPerMinuteMetricName} : {NumParcelPickupPerMinute},\n" +
                            $"{ParcelDeliveryPerMinuteMetricName} : {NumParcelDeliveryPerMinute}\n");

                          Partition.ReportLoad(new List<LoadMetric> {
                          new LoadMetric(UserCreationPerMinuteMetricName, NumUserCreationsPerMinute),
                          new LoadMetric(ParcelRequestCreationPerMinuteMetricName, NumParcelRequestCreationPerMinute),
                          new LoadMetric(ParcelRequestApprovalPerMinuteMetricName, NumParcelRequestApprovalPerMinute),
                          new LoadMetric(ParcelRequestDenialPerMinuteMetricName, NumParcelRequestDenialPerMinute),
                          new LoadMetric(ParcelPickupPerMinuteMetricName, NumParcelPickupPerMinute),
                          new LoadMetric(ParcelDeliveryPerMinuteMetricName, NumParcelDeliveryPerMinute)
                      });

                          NumUserCreationsPerMinute = 0;
                          NumParcelRequestCreationPerMinute = 0;
                          NumParcelRequestApprovalPerMinute = 0;
                          NumParcelRequestDenialPerMinute = 0;
                          NumParcelPickupPerMinute = 0;
                          NumParcelDeliveryPerMinute = 0;
                      }

                      await Task.Delay(LongerMetricsReportingInterval, cancellationToken);
                  }
              }, cancellationToken);
        }

        private async void DefineMetricsAndPolicies()
        {
            FabricClient fabricClient = new FabricClient();
            StatelessServiceUpdateDescription updateDescription = new StatelessServiceUpdateDescription();

            // METRICS
            RegisterMetrics(updateDescription);

            // SCALING
            RegisterScaling(updateDescription);

           // Update service with metrics and scaling information
           await fabricClient.ServiceManager.UpdateServiceAsync(Context.ServiceName, updateDescription);
        }

        private void RegisterMetrics(StatelessServiceUpdateDescription updateDescription)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, $"ManagementAPI {Context.InstanceId} registering metrics {Context.ServiceName}");

            StatelessServiceLoadMetricDescription RequestsPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = TotalRequestsPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription UserCreationPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = UserCreationPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription ParcelRequestCreationPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = ParcelRequestCreationPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription ParcelRequestApprovalPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = ParcelRequestApprovalPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription ParcelRequestDenialPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = ParcelRequestDenialPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription ParcelPickupPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = ParcelPickupPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };
            StatelessServiceLoadMetricDescription ParcelDeliveryPerMinuteMetric = new StatelessServiceLoadMetricDescription
            {
                Name = ParcelDeliveryPerMinuteMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };

            // Adding metrics to update description
            if (updateDescription.Metrics == null)
            {
                updateDescription.Metrics = new MetricsCollection();
            }
            updateDescription.Metrics.Add(RequestsPerMinuteMetric);
            updateDescription.Metrics.Add(UserCreationPerMinuteMetric);
            updateDescription.Metrics.Add(ParcelRequestCreationPerMinuteMetric);
            updateDescription.Metrics.Add(ParcelRequestApprovalPerMinuteMetric);
            updateDescription.Metrics.Add(ParcelRequestDenialPerMinuteMetric);
            updateDescription.Metrics.Add(ParcelPickupPerMinuteMetric);
            updateDescription.Metrics.Add(ParcelDeliveryPerMinuteMetric);
        }

        private void RegisterScaling(StatelessServiceUpdateDescription updateDescription)
        {
            PartitionInstanceCountScaleMechanism mechanism = new PartitionInstanceCountScaleMechanism
            {
                MaxInstanceCount = 10,
                MinInstanceCount = 1,
                ScaleIncrement = 1
            };

            // Scale is related to the total number of requests per minute
            AveragePartitionLoadScalingTrigger trigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = TotalRequestsPerMinuteMetricName,
                ScaleInterval = ScaleInterval,
                LowerLoadThreshold = 20.0,
                UpperLoadThreshold = 40.0
            };

            // Policy consists of scaling mechanism and scaling trigger
            ScalingPolicyDescription policy = new ScalingPolicyDescription(mechanism, trigger);

            // Adding policy to update description
            if (updateDescription.ScalingPolicies == null)
            {
                updateDescription.ScalingPolicies = new List<ScalingPolicyDescription>();
            }
            updateDescription.ScalingPolicies.Add(policy);
        }

        public static void RegisterGeneralRequestForMetrics() {
            lock (metricsLock)
            {
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterUserCreationsForMetrics()
        {
            lock (metricsLock)
            {
                NumUserCreationsPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterParcelRequestCreationForMetrics()
        {
            lock (metricsLock)
            {
                NumParcelRequestCreationPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterParcelRequestApprovalForMetrics()
        {
            lock (metricsLock)
            {
                NumParcelRequestApprovalPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterParcelRequestDenialForMetrics()
        {
            lock (metricsLock)
            {
                NumParcelRequestDenialPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterParcelPickupForMetrics()
        {
            lock (metricsLock)
            {
                NumParcelPickupPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }

        public static void RegisterParcelDeliveryForMetrics()
        {
            lock (metricsLock)
            {
                NumParcelDeliveryPerMinute++;
                TotalNumRequestsThese30s++;
            }
        }
    }
}
