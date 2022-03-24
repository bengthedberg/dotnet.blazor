all : clean restore build publish

clean:
	dotnet clean

restore:
	dotnet restore

build: 
	dotnet build

publish:
	dotnet publish -c Release -r linux-x64

run:
	dotnet run --project src/dotnet.blazor.client/dotnet.blazor.client.csproj

watch:
	dotnet watch --project src/dotnet.blazor.client/dotnet.blazor.client.csproj run

local-api:
	json-server --watch src/dotnet.blazor.client/wwwroot/sample-data/data.json

slow-local-api:
	json-server --watch --delay 1000 src/dotnet.blazor.client/wwwroot/sample-data/data.json