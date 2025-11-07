## Live URL
Base:  
https://weather-func-2xio34typ47vw.azurewebsites.net  

# Test
curl.exe -X POST https://weather-func-2xio34typ47vw.azurewebsites.net/api/start-job

curl.exe https://weather-func-2xio34typ47vw.azurewebsites.net/api/job-status/{job-id}

# Deploying 
dotnet build

./deploy.ps1


# Local Emulator
npm config get prefix

$env:PATH += "..." 

azurite.cmd

func.cmd start

curl.exe -X POST http://localhost:7071/api/start-job

curl.exe http://localhost:7071/api/job-status/{jobId}
