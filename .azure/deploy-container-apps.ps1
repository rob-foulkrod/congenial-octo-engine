$ErrorActionPreference = "Stop"

$subscriptionId = "96d0d57a-566f-4b73-bc46-345620f9a3e7"
$resourceGroup = "octo-engine-todo_group"
$location = "northcentralus"
$environmentName = "octo-engine-aca-env"
$acrName = "octoengineacr"
$acrLoginServer = "octoengineacr-bjeug7bydkbcdtdt.azurecr.io"
$identityName = "ua-id-8c52"
$webAppName = "octo-engine-web"
$workerAppName = "octo-engine-worker"
$webImage = "$acrLoginServer/octo-engine-web:v2"
$workerImage = "$acrLoginServer/octo-engine-worker:v2"

Set-Location (Resolve-Path (Join-Path $PSScriptRoot ".."))

Write-Host "Step 1 - Select the Azure subscription and clear any prior demo apps"
az account set --subscription $subscriptionId
if ($LASTEXITCODE -ne 0) { throw "Could not select subscription." }

az containerapp delete --resource-group $resourceGroup --name $webAppName --subscription $subscriptionId --yes --only-show-errors 2>$null
az containerapp delete --resource-group $resourceGroup --name $workerAppName --subscription $subscriptionId --yes --only-show-errors 2>$null

Write-Host "Step 2 - Build the .NET apps"
dotnet build .\src\Todo.Web\Todo.Web.csproj --configuration Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Web build failed." }
dotnet build .\src\Todo.Worker\Todo.Worker.csproj --configuration Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Worker build failed." }

Write-Host "Step 3 - Build and push the web image"
az acr login --name $acrName --subscription $subscriptionId
if ($LASTEXITCODE -ne 0) { throw "ACR login failed." }
docker build -t $webImage -f .\src\Todo.Web\Dockerfile .
if ($LASTEXITCODE -ne 0) { throw "Web image build failed." }
docker push $webImage
if ($LASTEXITCODE -ne 0) { throw "Web image push failed." }

Write-Host "Step 4 - Build and push the background worker image"
docker build -t $workerImage -f .\src\Todo.Worker\Dockerfile .
if ($LASTEXITCODE -ne 0) { throw "Worker image build failed." }
docker push $workerImage
if ($LASTEXITCODE -ne 0) { throw "Worker image push failed." }

Write-Host "Step 5 - Grant the user-assigned identity AcrPull on the existing ACR"
$identityId = az identity show `
    --resource-group $resourceGroup `
    --name $identityName `
    --subscription $subscriptionId `
    --query id `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not find identity $identityName." }

$principalId = az identity show `
    --resource-group $resourceGroup `
    --name $identityName `
    --subscription $subscriptionId `
    --query principalId `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not find principal ID for $identityName." }

$acrId = az acr show `
    --name $acrName `
    --subscription $subscriptionId `
    --query id `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not find ACR $acrName." }

$existingAcrPull = az role assignment list `
    --assignee $principalId `
    --scope $acrId `
    --role AcrPull `
    --query "[0].id" `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not check AcrPull role assignment." }

if (-not $existingAcrPull) {
    az role assignment create `
        --assignee-object-id $principalId `
        --assignee-principal-type ServicePrincipal `
        --role AcrPull `
        --scope $acrId `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Could not assign AcrPull." }
}

Write-Host "Step 6 - Create the Azure Container Apps environment"
$existingEnvironment = az containerapp env show `
    --resource-group $resourceGroup `
    --name $environmentName `
    --subscription $subscriptionId `
    --query name `
    -o tsv 2>$null

if (-not $existingEnvironment) {
    az containerapp env create `
        --resource-group $resourceGroup `
        --name $environmentName `
        --location $location `
        --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw "Could not create Container Apps environment." }
}

Write-Host "Step 7 - Create the public frontend Container App with managed-identity ACR auth"
az containerapp create `
    --resource-group $resourceGroup `
    --name $webAppName `
    --environment $environmentName `
    --image $webImage `
    --user-assigned $identityId `
    --registry-server $acrLoginServer `
    --registry-identity $identityId `
    --ingress external `
    --target-port 8080 `
    --revisions-mode Single `
    --cpu 0.5 `
    --memory 1.0Gi `
    --min-replicas 1 `
    --max-replicas 2 `
    --env-vars ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Production WorkerGrpc__Address=http://octo-engine-worker `
    --query "properties.provisioningState" `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not create frontend app." }

Write-Host "Step 8 - Apply the frontend YAML"
az containerapp update `
    --resource-group $resourceGroup `
    --name $webAppName `
    --yaml .\.azure\aca-web.yaml `
    --query "properties.provisioningState" `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not update frontend from YAML." }

Write-Host "Step 9 - Create the private background Container App with managed-identity ACR auth"
az containerapp create `
    --resource-group $resourceGroup `
    --name $workerAppName `
    --environment $environmentName `
    --image $workerImage `
    --user-assigned $identityId `
    --registry-server $acrLoginServer `
    --registry-identity $identityId `
    --ingress internal `
    --target-port 8080 `
    --transport http2 `
    --revisions-mode Single `
    --cpu 0.25 `
    --memory 0.5Gi `
    --min-replicas 1 `
    --max-replicas 1 `
    --env-vars DOTNET_ENVIRONMENT=Production ASPNETCORE_URLS=http://+:8080 Worker__ServiceName=octo-engine-worker Worker__IntervalSeconds=15 "Worker__Scenario=Background todo digest processor for the ACA classroom demo." `
    --query "properties.provisioningState" `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not create worker app." }

Write-Host "Step 10 - Apply the worker YAML"
az containerapp update `
    --resource-group $resourceGroup `
    --name $workerAppName `
    --yaml .\.azure\aca-worker.yaml `
    --query "properties.provisioningState" `
    -o tsv
if ($LASTEXITCODE -ne 0) { throw "Could not update worker from YAML." }

Write-Host "Step 11 - Verify the public frontend and worker logs"
$fqdn = az containerapp show `
    --resource-group $resourceGroup `
    --name $webAppName `
    --subscription $subscriptionId `
    --query "properties.configuration.ingress.fqdn" `
    -o tsv
if ($LASTEXITCODE -ne 0 -or -not $fqdn) { throw "Could not get frontend URL." }

$health = Invoke-WebRequest -Uri "https://$fqdn/healthz" -UseBasicParsing -TimeoutSec 60
$workerDependency = Invoke-WebRequest -Uri "https://$fqdn/workerz" -UseBasicParsing -TimeoutSec 60
Start-Sleep -Seconds 5

$workerLogCommand = 'az containerapp logs show --resource-group "octo-engine-todo_group" --name "octo-engine-worker" --subscription "96d0d57a-566f-4b73-bc46-345620f9a3e7" --tail 120'
Write-Host "Frontend URL: https://$fqdn/"
Write-Host "Health check: $($health.Content)"
Write-Host "Worker dependency check: $($workerDependency.Content)"
Write-Host "Worker log command:"
Write-Host $workerLogCommand

az containerapp logs show `
    --resource-group $resourceGroup `
    --name $workerAppName `
    --subscription $subscriptionId `
    --tail 120
if ($LASTEXITCODE -ne 0) { throw "Could not read worker logs." }
