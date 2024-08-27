using System;
using Runtime.Common;
using VitalRouter;

namespace Runtime.Commands
{
    public sealed class FeatureBindingRequestCommand : ICommand
    {
        public FeatureHandler FeatureHandler { get; private set; }
        public string ID { get; private set; }

        public int DependenciesCounts { get; private set; }

        public void Initialize(FeatureHandler featureHandler, string id)
        {
            FeatureHandler = featureHandler;
            ID = id;
            
            DependenciesCounts = 0;
        }

        public void Use()
        {
            DependenciesCounts++;
        }
    }
}