FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY src/Catdocs/*.csproj ./src/Catdocs/
WORKDIR /app/src/Catdocs
RUN dotnet restore

WORKDIR /app
COPY . ./
WORKDIR /app/src/Catdocs
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "Catdocs.dll"]