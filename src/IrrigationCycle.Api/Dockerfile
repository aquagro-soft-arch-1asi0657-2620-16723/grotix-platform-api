# Build context: Grotix_Web_Services/
# docker build -f src/IrrigationCycle.Api/Dockerfile -t grotix-irrigation-api .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/IrrigationCycle.Api/IrrigationCycle.Api.csproj",                                    "src/IrrigationCycle.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj",                                               "src/BuildingBlocks/"]
COPY ["src/Grotix.Shared/Grotix.Shared.csproj",                                                 "src/Grotix.Shared/"]
COPY ["src/Grotix.IAM/Grotix.IAM.csproj",                                                       "src/Grotix.IAM/"]
COPY ["src/Grotix.Profiles/Grotix.Profiles.csproj",                                             "src/Grotix.Profiles/"]
COPY ["src/Grotix.CultivationArea/Grotix.CultivationArea.csproj",                               "src/Grotix.CultivationArea/"]
COPY ["src/Grotix.IrrigationCycle/Grotix.IrrigationCycle.csproj",                               "src/Grotix.IrrigationCycle/"]
COPY ["src/Grotix.Persistence/Grotix.Persistence.csproj",                                       "src/Grotix.Persistence/"]
COPY ["src/Grotix.Persistence.IrrigationCycle/Grotix.Persistence.IrrigationCycle.csproj",       "src/Grotix.Persistence.IrrigationCycle/"]
COPY ["src/Grotix.Contracts.Auth/Grotix.Contracts.Auth.csproj",                                 "src/Grotix.Contracts.Auth/"]
COPY ["src/Grotix.Contracts.Profiles/Grotix.Contracts.Profiles.csproj",                         "src/Grotix.Contracts.Profiles/"]

COPY ["src/Grotix.Contracts.Integration/Grotix.Contracts.Integration.csproj", "src/Grotix.Contracts.Integration/"]
COPY ["src/Grotix.HardwareDevice/Grotix.HardwareDevice.csproj", "src/Grotix.HardwareDevice/"]
COPY ["src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj", "src/Grotix.Persistence.HardwareDevice/"]
COPY ["src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj", "src/Grotix.Persistence.Telemetry/"]
COPY ["src/Grotix.Telemetry/Grotix.Telemetry.csproj", "src/Grotix.Telemetry/"]

RUN dotnet restore "src/IrrigationCycle.Api/IrrigationCycle.Api.csproj"

COPY . .

RUN dotnet publish "src/IrrigationCycle.Api/IrrigationCycle.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IrrigationCycle.Api.dll"]
