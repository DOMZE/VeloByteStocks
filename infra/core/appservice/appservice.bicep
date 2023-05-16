param name string
param location string = resourceGroup().location
param tags object = {}

// Reference Properties
param appServicePlanId string

// Runtime Properties
param runtimeVersion string
param runtimeNameAndVersion string = 'dotnetcore|${runtimeVersion}'

// Microsoft.Web/sites Properties
param kind string = 'app,linux'

// Microsoft.Web/sites/config
param alwaysOn bool = true
param clientAffinityEnabled bool = false
param functionAppScaleLimit int = -1
param linuxFxVersion string = runtimeNameAndVersion
param minimumElasticInstanceCount int = -1
param numberOfWorkers int = -1
param ftpsState string = 'FtpsOnly'

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: tags
  kind: kind
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      alwaysOn: alwaysOn
      ftpsState: ftpsState
      minTlsVersion: '1.2'
      numberOfWorkers: numberOfWorkers != -1 ? numberOfWorkers : null
      minimumElasticInstanceCount: minimumElasticInstanceCount != -1 ? minimumElasticInstanceCount : null
      use32BitWorkerProcess: false
      functionAppScaleLimit: functionAppScaleLimit != -1 ? functionAppScaleLimit : null
    }
    clientAffinityEnabled: clientAffinityEnabled
    httpsOnly: true
  }

  identity: {
    type: 'SystemAssigned'
  }
}

output identityPrincipalId string = appService.identity.principalId
output name string = appService.name
output uri string = 'https://${appService.properties.defaultHostName}'
output hostname string = appService.properties.defaultHostName
