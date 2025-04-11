FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
ARG BUILD_CONFIGURATION=Release

COPY ["Chillde.API/Chillde.API.csproj", "Chillde.API/"]
COPY Chillde.API/appsettings.json ./appsettings.json
COPY ["Chillde.Services/Chillde.Services.csproj", "Chillde.Services/"]
COPY ["Chillde.Repositories/Chillde.Repositories.csproj", "Chillde.Repositories/"]
RUN dotnet restore Chillde.API/Chillde.API.csproj
COPY . .
WORKDIR "/Chillde.API"
RUN dotnet build Chillde.API.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
WORKDIR /Chillde.API
RUN dotnet publish Chillde.API.csproj -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PORT=8080
ENTRYPOINT ["dotnet", "Chillde.API.dll"]

