# Phase de base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Phase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Configuration/ Configuration/

# Copier les projets
COPY NotesService/NotesService.csproj NotesService/

RUN dotnet restore NotesService/NotesService.csproj

# Copier tout le code
COPY NotesService/ NotesService/
COPY Utilitaires/ Utilitaires/

WORKDIR /src/NotesService
RUN dotnet build NotesService.csproj -c Release -o /app/build

# Phase de publication
FROM build AS publish
RUN dotnet publish NotesService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Phase finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotesService.dll"]
