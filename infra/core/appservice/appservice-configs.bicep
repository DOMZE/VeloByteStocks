param keyVaultName string = ''
param keyVaultConnectionStringSecretName string = ''
param appServiceName string = ''
param applicationInsightsName string = ''
param appSettings object = {}
param easyAuthConfig object = {}
param easyAuthKey string = 'AAD-API-AAD-CLIENT-SECRET'
param easyAuthConfigKey string = 'EASYAUTH_AAD_CLIENT_SECRET'

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = if (!empty(applicationInsightsName)) {
  name: applicationInsightsName
}

resource sqlAzureConnectionStringSercret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: easyAuthKey
  properties: {
    value: easyAuthConfig.clientSecret
  }
}

resource configAppSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: appService
  name: 'appsettings'
  properties: union(appSettings,
    !empty(applicationInsightsName) ? { APPLICATIONINSIGHTS_CONNECTION_STRING: applicationInsights.properties.ConnectionString } : {},
    !empty(easyAuthConfig) ? { '${easyAuthConfigKey}': '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/${easyAuthKey})' } : {},
    { ConnectionStrings__VeloByte: '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/${keyVaultConnectionStringSecretName})' }
  )
}

resource configLogs 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: appService
  name: 'logs'
  properties: {
    applicationLogs: { fileSystem: { level: 'Verbose' } }
    detailedErrorMessages: { enabled: true }
    failedRequestsTracing: { enabled: true }
    httpLogs: { fileSystem: { enabled: true, retentionInDays: 1, retentionInMb: 35 } }
  }
  dependsOn: [
    configAppSettings
  ]
}

resource easyAuth 'Microsoft.Web/sites/config@2022-03-01' = if (!empty(easyAuthConfig)) {
  name: 'authsettingsV2'
  kind: 'string'
  parent: appService
  properties: {
    globalValidation: {
      requireAuthentication: true
      unauthenticatedClientAction: 'Return401'
      excludedPaths: []
    }
    httpSettings: {
      requireHttps: true
    }
    login: {
      tokenStore: {
        enabled: true
      }
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        login: {
          loginParameters: []
        }
        registration: {
          clientId: easyAuthConfig.clientId
          // The app setting name that contains the client secret.	
          clientSecretSettingName: easyAuthConfigKey
          openIdIssuer: easyAuthConfig.issuer
        }
      }
    }
  }
}
