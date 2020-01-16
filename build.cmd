
set version=%1
set key=%2

cd %~dp0
dotnet build magic.lambda.auth/magic.lambda.auth.csproj --configuration Release --source https://api.nuget.org/v3/index.json
dotnet nuget push magic.lambda.auth/bin/Release/magic.lambda.auth.%version%.nupkg -k %key% -s https://api.nuget.org/v3/index.json
