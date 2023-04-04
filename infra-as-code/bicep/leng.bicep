// Define parameter values
param location string = resourceGroup().location
param appName string = 'libraryofleng'

// Database parameters
param databaseAdminLogin string
@secure()
param databaseAdminPassword string

// Create app service plan
resource asp 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: '${appName}-asp'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
}

// Create app service
resource web 'Microsoft.Web/sites@2021-02-01' = {
  name: '${appName}-web'
  location: location
  kind: 'app'
  properties: {
    serverFarmId: asp.id
    httpsOnly: true
  }
}

// Create function app
resource function 'Microsoft.Web/sites@2021-02-01' = {
  name: '${appName}-function'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: asp.id
    httpsOnly: true
  }
}

// Create SQL Server
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${appName}-sql'
  location: location
  properties: {
    administratorLogin: databaseAdminLogin
    administratorLoginPassword: databaseAdminPassword
  }
}

// Create firewall rule for the SQL Server
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  name: 'AllowAllWindowsAzureIps'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Create SQL Database
resource sqlDB 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: '${appName}-db'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    requestedBackupStorageRedundancy: 'local'
  }
}

