# === Phase de base ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# === Phase de build ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1) Copier les .csproj pour tirer parti du cache Docker sur dotnet restore
COPY NotesService/NotesService.csproj NotesService/
COPY Utilitaires/Commons.csproj Utilitaires/Commons.csproj

# 2) Restaurer les dépendances
RUN dotnet restore NotesService/NotesService.csproj

# 3) Copier tout le code source
COPY NotesService/ NotesService/
COPY Utilitaires/ Utilitaires/

# 4) Compiler en Release
WORKDIR /src/NotesService
RUN dotnet build NotesService.csproj -c Release -o /app/build

# === Phase de publication ===
FROM build AS publish
WORKDIR /src/NotesService
RUN dotnet publish NotesService.csproj -c Release -o /app/publish /p:UseAppHost=false

# === Phase finale ===
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotesService.dll"]
