# src/Library.NotificationService/Dockerfile

# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie SEULEMENT le .csproj d'abord — optimisation de cache Docker layer
COPY ["src/Library.NotificationService/Library.NotificationService.csproj", "src/Library.NotificationService/"]
COPY ["src/Library.Domain/Library.Domain.csproj", "src/Library.Domain/"]

RUN dotnet restore "src/Library.NotificationService/Library.NotificationService.csproj"

# Copie tout le reste du code source
COPY . .

WORKDIR /src/src/Library.NotificationService
RUN dotnet build "Library.NotificationService.csproj" -c Release -o /app/build

# Stage 2 — Publish
FROM build AS publish
RUN dotnet publish "Library.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3 — Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Library.NotificationService.dll"]