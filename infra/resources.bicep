targetScope = 'resourceGroup'

param location string
param resourceNamePrefix string
param appNameSuffix string

resource productionPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'asp-${resourceNamePrefix}-prod'
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource productionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: '${resourceNamePrefix}-prod-${appNameSuffix}'
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: productionPlan.id
    reserved: true
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
}

resource productionFtpPolicy 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  name: 'ftp'
  parent: productionApp
  properties: {
    allow: false
  }
}

resource productionScmPolicy 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
  name: 'scm'
  parent: productionApp
  properties: {
    allow: false
  }
}

output productionAppName string = productionApp.name
