// Define parameter values
param location string = resourceGroup().location
param appName string = 'libraryofleng'

// Database parameters
param databaseAdminLogin string
@secure()
param databaseAdminPassword string

// Create appInsights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${appName}-Insights'
  kind: 'web'
  location: location
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Create storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${appName}storage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

// Create app service plan
resource asp 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: '${appName}-asp'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
}

// Create app service - add appsetting with reference to keyvault to read database admin login and password
resource web 'Microsoft.Web/sites@2021-02-01' = {
  name: '${appName}-web'
  location: location
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: asp.id
    httpsOnly: true  
    siteConfig: {
      appSettings: [
        {
          name: 'AzureAdB2C:TestSetting'
          value: '1234567890'
        }
        {
          name: 'sqlConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultSecretDatabaseConnectionString.properties.secretUriWithVersion})'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
  }
}

// Create function app
resource function 'Microsoft.Web/sites@2021-02-01' = {
  name: '${appName}-function'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }  
  properties: {
    serverFarmId: asp.id
    httpsOnly: true  
    siteConfig: {
      appSettings: [
        {
          name: 'sqlConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultSecretDatabaseConnectionString.properties.secretUriWithVersion})'
        }        
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
    }
  }
}

// Create SQL Server
// Allow access to the SQL Server from the web app
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${appName}-sql'
  location: location
  identity: {
      type: 'SystemAssigned'
  }
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

// Create Azure Keyvault
resource keyVault 'Microsoft.KeyVault/vaults@2022-11-01' = {
  name: '${appName}-kv'
  location: location
  properties: {
  	sku: {
	    name: 'standard'
	    family: 'A'
  	}
	  tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: '88b345ba-5932-475e-8e56-92b95b677901'
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
  }
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-11-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: web.identity.principalId
        permissions: {
          secrets: ['get']
        }
      }, {
        tenantId: subscription().tenantId
        objectId: function.identity.principalId
        permissions: {
          secrets: ['get']
        }
      }
    ]
  }
}

// Add secret to KeyVault - database admin login
resource keyVaultSecretDatabaseAdminLogin 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
  name: 'databaseAdminLogin'
  parent: keyVault
  properties: {
	value: databaseAdminLogin
  }
}

// Add secret to KeyVault - database password
resource keyVaultSecretDatabaseAdminPassword 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
  name: 'databaseAdminPassword'
  parent: keyVault
  properties: {
    value: databaseAdminPassword
  }
}

// Add secret to KeyVault - SQL Connection string
resource keyVaultSecretDatabaseConnectionString 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
  name: 'sqlConnectionString'
  parent: keyVault
  properties: {
    value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDB.name};Persist Security Info=False;User ID=${databaseAdminLogin};Password=${databaseAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}
