@description('Name of the Azure Function App')
param functionAppName string = 'weather-func-${uniqueString(resourceGroup().id)}'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Storage account name (must be globally unique)')
param storageAccountName string = 'wstrg${uniqueString(resourceGroup().id)}'

@description('App Service plan name')
param hostingPlanName string = 'weather-func-plan'

@description('Azure Functions runtime version')
param functionsVersion string = '4'

@description('Runtime stack for Function App')
param runtime string = 'dotnet-isolated'

/* STORAGE ACCOUNT */
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/images'
  properties: {
    publicAccess: 'None'
  }
}


/* Queues for background processing */
resource queueJobs 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/jobs-queue'
}
resource queueImages 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/image-processing-queue'
}

/* HOSTING PLAN */
resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

/* FUNCTION APP */
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: runtime
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(resourceId('Microsoft.Storage/storageAccounts', storageAccount.name), '2023-01-01').keys[0].value};EndpointSuffix=core.windows.net'
        }
      ]
    }
  }
  dependsOn: [
    storageAccount
    hostingPlan
  ]
}

output storageAccountName string = storageAccount.name
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
