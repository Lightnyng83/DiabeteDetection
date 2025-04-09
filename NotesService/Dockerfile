# Phase de base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Phase de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copier les fichiers csproj pour NotesService et PatientService
COPY ["NotesService/NotesService.csproj", "NotesService/"]
COPY ["PatientService/PatientService.csproj", "PatientService/"]

# Restaurer les dépendances de NotesService (qui inclut la référence à PatientService)
RUN dotnet restore "NotesService/NotesService.csproj"

# Copier l'intégralité de la solution
COPY . .

# Construire NotesService
WORKDIR "/src/NotesService"
RUN dotnet build "NotesService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Phase de publication
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NotesService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Phase finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotesService.dll"]
