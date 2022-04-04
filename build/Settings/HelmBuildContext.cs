using System;
using System.IO;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Frosting;
using Newtonsoft.Json;

public class HelmBuildContext : FrostingContext
{
  public BuildSettings Settings { get; }

  public HelmBuildContext(ICakeContext context) : base(context)
  {
    var requiredVariables = new string[]
    {
      "projectName",
      "namespace",
      "servicePrincipalUsername",
      "servicePrincipalPassword",
      "servicePrincipalTenant",
      "preview",
      "registry",
      "clusterName",
      "resourceGroup",
      "mongodbConnectionString",
      "serviceBusConnectionString",
    };
    bool didError = false;
    foreach (var variable in requiredVariables)
    {
      if (string.IsNullOrWhiteSpace(context.Environment.GetEnvironmentVariable(variable)) && string.IsNullOrWhiteSpace(context.Arguments.GetArgument(variable)))
      {
        context.Error("Missing variable: {0}", variable);
        didError = true;
      }
    }

    if (didError)
    {
      throw new Exception("Missing required arguments");
    }

    Settings = new BuildSettings
    {
      Azure = new AzureSettings
      {
        Cluster = new Cluster
        {
          Name = context.Environment.GetEnvironmentVariable(WellKnownVariables.ClusterName) ?? context.Arguments.GetArgument(WellKnownVariables.ClusterName),
          ResourceGroup = context.Environment.GetEnvironmentVariable(WellKnownVariables.ResourceGroup) ?? context.Arguments.GetArgument(WellKnownVariables.ResourceGroup)
        },
        ServicePrincipal = new ServicePrincipal
        {
          Username = context.Arguments.GetArgument("servicePrincipalUsername"),
          Password = context.Arguments.GetArgument("servicePrincipalPassword"),
          Tenant = context.Arguments.GetArgument("servicePrincipalTenant")
        }
      }
    };
    ProjectName = context.Arguments.GetArgument(WellKnownVariables.ProjectName);
    Preview = bool.TryParse(context.Arguments.GetArgument("preview"), out var preview) && preview;
  }

  public string ProjectName { get; }

  public bool Preview { get; }


}

public class WellKnownVariables
{
  public const string ClusterName = "clusterName";
  public const string ResourceGroup = "resourceGroup";
  public const string ProjectName = "projectName";
}

