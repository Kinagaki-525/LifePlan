targetScope = 'resourceGroup'

param webAppName string
param hostName string
param thumbprint string

resource webApp 'Microsoft.Web/sites@2024-04-01' existing = {
  name: webAppName
}

resource hostNameBinding 'Microsoft.Web/sites/hostNameBindings@2024-04-01' = {
  name: hostName
  parent: webApp
  properties: {
    siteName: webAppName
    hostNameType: 'Verified'
    sslState: 'SniEnabled'
    thumbprint: thumbprint
  }
}
