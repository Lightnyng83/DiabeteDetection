# Base d'exécution
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PatientService/PatientService.csproj", "PatientService/"]
COPY ["Utilitaires/Utilitaires.csproj", "Utilitaires/"]

RUN dotnet restore "PatientService/PatientService.csproj"

COPY . .
WORKDIR "/src/PatientService"
RUN dotnet build "PatientService.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "PatientService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PatientService.dll"]
