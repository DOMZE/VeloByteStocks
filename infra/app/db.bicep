param name string
param location string = resourceGroup().location
param tags object = {}

param databaseName string = ''
param appUser string = 'appUser'
param keyVaultName string
param sqlADAdministrator object = {}

@secure()
param sqlAdminPassword string
@secure()
param appUserPassword string

// Because databaseName is optional in main.bicep, we make sure the database name is set here.
var defaultDatabaseName = 'VeloByte'
var actualDatabaseName = !empty(databaseName) ? databaseName : defaultDatabaseName

module sqlServer '../core/database/sqlserver.bicep' = {
  name: 'sqlserver'
  params: {
    name: name
    location: location
    tags: tags
    appUser: appUser
    appUserPassword: appUserPassword
    databaseName: actualDatabaseName
    keyVaultName: keyVaultName
    sqlAdminPassword: sqlAdminPassword
    sqlADAdministrator: sqlADAdministrator
  }
}

output connectionStringKey string = sqlServer.outputs.connectionStringKey
output databaseName string = sqlServer.outputs.databaseName
