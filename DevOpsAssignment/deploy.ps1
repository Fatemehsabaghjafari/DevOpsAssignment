param(
    [string]$resourceGroup = "WeatherFunctionRG",
    [string]$location = "francecentral"  
)
# ["swedencentral","spaincentral","francecentral","italynorth","norwayeast"]

# Stop on errors
$ErrorActionPreference = "Stop"

Write-Host "Creating resource group '$resourceGroup' in $location ..."
az group create --name $resourceGroup --location $location | Out-Null

Write-Host "Deploying Bicep infrastructure ..."
az deployment group create `
  --name main `
  --resource-group $resourceGroup `
  --template-file ./main.bicep `
  --parameters location=$location `
  --output json


# Retrieve outputs
$storageAccount = az deployment group show -g $resourceGroup -n main --query "properties.outputs.storageAccountName.value" -o tsv
$functionAppUrl = az deployment group show -g $resourceGroup -n main --query "properties.outputs.functionAppUrl.value" -o tsv

Write-Host "Storage Account: $storageAccount"
Write-Host "Function App URL: $functionAppUrl`n"

# Build and package the Function App
Write-Host "Building the project..."
dotnet publish -c Release

# Define paths
$framework = "net8.0"
$publishPath = Join-Path (Get-Location) "bin\Release\$framework\publish"
$zipPath = Join-Path $publishPath "app.zip"

Write-Host "Creating deployment package..."
if (!(Test-Path $publishPath)) {
    Write-Error "Publish folder not found: $publishPath"
    exit 1
}

if (Test-Path $zipPath) { Remove-Item $zipPath }
Push-Location $publishPath
Compress-Archive -Path * -DestinationPath $zipPath -Force
Pop-Location
Write-Host "Package created at: $zipPath"

# Deploy Function App Code
Write-Host "Deploying code to Function App..."
$functionName = az functionapp list -g $resourceGroup --query "[0].name" -o tsv

if (-not $functionName) {
    Write-Error "No Function App found in resource group $resourceGroup"
    exit 1
}

az functionapp deployment source config-zip `
  --name $functionName `
  --resource-group $resourceGroup `
  --src $zipPath | Out-Null

Write-Host "Deployment completed successfully!"
Write-Host "Your Function App is available at: $functionAppUrl"
