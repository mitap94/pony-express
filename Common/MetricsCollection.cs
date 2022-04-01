using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric.Description;
using System.Text;

namespace Common
{
    public class MetricsCollection : KeyedCollection<string, ServiceLoadMetricDescription>
    {
        protected override string GetKeyForItem(ServiceLoadMetricDescription metricDescription)
        {
            return metricDescription.Name;
        }
    }
}
