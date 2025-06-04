########################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID

WORKDIR /app

EXPOSE 8080

EXPOSE 8081
########################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/Conways.GameOfLife.API/", "Conways.GameOfLife.API/"]
COPY ["src/Conways.GameOfLife.Infrastructure/", "Conways.GameOfLife.Infrastructure/"]
COPY ["src/Conways.GameOfLife.ServiceDefaults/", "Conways.GameOfLife.ServiceDefaults/"]
COPY ["src/Conways.GameOfLife.Domain/", "Conways.GameOfLife.Domain/"]
COPY ["Directory.Build.props", "Directory.Build.props"] 
COPY ["Directory.Packages.props", "Directory.Packages.props"]

RUN dotnet restore Conways.GameOfLife.API/Conways.GameOfLife.API.csproj
RUN dotnet build Conways.GameOfLife.API/Conways.GameOfLife.API.csproj -c "$BUILD_CONFIGURATION" -o /app/build
########################################
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish Conways.GameOfLife.API/Conways.GameOfLife.API.csproj -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false
########################################
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Conways.GameOfLife.API.dll"]
