targetScope = 'subscription'

@description('Azure region for all resources.')
param location string

@description('Resource group name for the LifePlan application.')
param resourceGroupName string

@description('Prefix used for App Service resource names.')
param resourceNamePrefix string

@description('Custom host names bound to the production Web App with a free managed certificate. DNS records must exist before deployment.')
param customHostNames string[] = []

var appNameSuffix = uniqueString(subscription().id, resourceGroupName)

resource lifePlanResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
}

module lifePlanResources './resources.bicep' = {
  name: 'lifeplan-resources'
  scope: lifePlanResourceGroup
  params: {
    location: location
    resourceNamePrefix: resourceNamePrefix
    appNameSuffix: appNameSuffix
    customHostNames: customHostNames
  }
}

output productionAppName string = lifePlanResources.outputs.productionAppName
