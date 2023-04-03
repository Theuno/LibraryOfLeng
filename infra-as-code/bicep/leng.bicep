// Define parameter values
param location string = resourceGroup().location
param appName string = 'libraryofleng'

// Database parameters
param databaseLogin string
param databaseSid string
param databaseTid string


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
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: '${databaseLogin}'
      principalType: 'User'
      sid: '${databaseSid}'
      tenantId: '${databaseTid}'
    }

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

// Create key vault
// resource keyVault 'Microsoft.KeyVault/vaults@2021-04-01-preview' = {
//   name: '${appName}-kv'
//   location: location
//   properties: {
//     sku: {
//       family: 'A'
//       name: 'standard'
//     }
//     tenantId: subscription().tenantId
//     accessPolicies: [
//       {
//         tenantId: subscription().tenantId
//         objectId: reference(resourceGroup().id, '2021-04-01', 'Full').outputs.objectId
//         permissions: {
//           secrets: [
//             'get'
//             'set'
//           ]
//         }
//       }
//     ]
//   }
// }
