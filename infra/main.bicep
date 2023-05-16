targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@description('Primary location for all resources')
param location string

@description('The resource group name where the resources will be deployed')
param resourceGroupName string

param apiServiceName string = ''
param applicationInsightsDashboardName string = ''
param applicationInsightsName string = ''
param appServicePlanName string = ''
param keyVaultName string = ''
param logAnalyticsName string = ''
param sqlServerName string = ''
param sqlDatabaseName string = ''
param sqlADAdministrator object = {}
param easyAuthConfig object = {}

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

@secure()
@description('Application user password')
param appUserPassword string

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Store secrets in a keyvault
module keyVault './core/security/keyvault.bicep' = {
  name: 'keyvault'
  scope: rg
  params: {
    name: !empty(keyVaultName) ? keyVaultName : '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
  }
}

// Create an App Service Plan to group applications under the same payment plan and SKU
module appServicePlan './core/appservice/appserviceplan.bicep' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: !empty(appServicePlanName) ? appServicePlanName : '${abbrs.webServerFarms}${resourceToken}'
    location: location
    sku: {
      name: 'B2'
    }
  }
}

// The API
module api './core/appservice/appservice.bicep' = {
  name: 'appservice'
  scope: rg
  params: {
    name: !empty(apiServiceName) ? apiServiceName : '${abbrs.webSitesAppService}${resourceToken}'
    location: location
    appServicePlanId: appServicePlan.outputs.id
    runtimeVersion: '7.0'
  }
}

// Give the API access to KeyVault
module apiKeyVaultAccess './core/security/keyvault-access.bicep' = {
  name: 'api-keyvault-access'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.name
    principalId: api.outputs.identityPrincipalId
  }
}

// The API configs
module apiConfigs './core/appservice/appservice-configs.bicep' = {
  name: 'appservice-configs'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.name
    keyVaultConnectionStringSecretName: sqlServer.outputs.connectionStringKey
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    appServiceName: api.outputs.name
    easyAuthConfig: easyAuthConfig
  }
  dependsOn: [
    api
    apiKeyVaultAccess
    keyVault
    sqlServer
    monitoring
  ]
}

// Monitor application with Azure Monitor
module monitoring './core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    location: location
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: !empty(applicationInsightsDashboardName) ? applicationInsightsDashboardName : '${abbrs.portalDashboards}${resourceToken}'
  }
}

// Create the load test
module loadtest './app/loadtest.bicep' = {
  name: 'loadtest'
  scope: rg
  params: {
    name: '${abbrs.loadtest}${resourceToken}'
    location: location
  }
}

// The application database
module sqlServer './app/db.bicep' = {
  name: 'sqldatabase'
  scope: rg
  params: {
    appUserPassword: appUserPassword
    name: !empty(sqlServerName) ? sqlServerName : '${abbrs.sqlServers}${resourceToken}'
    databaseName: sqlDatabaseName
    location: location
    sqlAdminPassword: sqlAdminPassword
    keyVaultName: keyVault.outputs.name
    sqlADAdministrator: sqlADAdministrator
  }
}

// Data outputs
output AZURE_SQL_CONNECTION_STRING_KEY string = sqlServer.outputs.connectionStringKey

// App outputs
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.endpoint
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_LOAD_TEST_NAME string = loadtest.name
output AZURE_LOAD_TEST_HOST string = api.outputs.uri
