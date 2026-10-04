targetScope = 'resourceGroup'

param location string
param resourceNamePrefix string
param appNameSuffix string
param customHostNames string[]

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

// マネージド証明書はホスト名のバインド後でないと作成できないため、
// SSL なしでバインド → 証明書作成 → SNI SSL へ更新、の順に作成する。
// 同じ Web App への更新が並行すると Conflict になるため、各段階は前の段階の完了を待つ。
@batchSize(1)
resource productionHostNameBindings 'Microsoft.Web/sites/hostNameBindings@2024-04-01' = [for hostName in customHostNames: {
  name: hostName
  parent: productionApp
  properties: {
    siteName: productionApp.name
    hostNameType: 'Verified'
  }
  dependsOn: [
    productionFtpPolicy
    productionScmPolicy
  ]
}]

@batchSize(1)
resource productionCertificates 'Microsoft.Web/certificates@2024-04-01' = [for hostName in customHostNames: {
  name: hostName
  location: location
  properties: {
    serverFarmId: productionPlan.id
    canonicalName: hostName
  }
  dependsOn: [
    productionHostNameBindings
  ]
}]

@batchSize(1)
module productionHostNameSsl './hostname-ssl.bicep' = [for (hostName, i) in customHostNames: {
  name: 'hostname-ssl-${replace(hostName, '.', '-')}'
  params: {
    webAppName: productionApp.name
    hostName: hostName
    thumbprint: productionCertificates[i].properties.thumbprint
  }
  dependsOn: [
    productionCertificates
  ]
}]

output productionAppName string = productionApp.name
