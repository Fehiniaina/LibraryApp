# src/Library.Api/Dockerfile

# Stage 1 — Build (SDK complet, lourd, jeté après le build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie SEULEMENT les .csproj d'abord — optimisation de cache Docker layer
COPY ["src/Library.Api/Library.Api.csproj", "src/Library.Api/"]
COPY ["src/Library.Application/Library.Application.csproj", "src/Library.Application/"]
COPY ["src/Library.Infrastructure/Library.Infrastructure.csproj", "src/Library.Infrastructure/"]
COPY ["src/Library.Domain/Library.Domain.csproj", "src/Library.Domain/"]

RUN dotnet restore "src/Library.Api/Library.Api.csproj"

# Copie TOUT le reste du code source
COPY . .

WORKDIR /src/src/Library.Api
RUN dotnet build "Library.Api.csproj" -c Release -o /app/build

# Stage 2 — Publish
FROM build AS publish
RUN dotnet publish "Library.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3 — Runtime (SDK ASP.NET seul, PAS le SDK complet — beaucoup plus léger)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Library.Api.dll"]