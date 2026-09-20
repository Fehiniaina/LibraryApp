# src/Library.Gateway/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Library.Gateway/Library.Gateway.csproj", "src/Library.Gateway/"]

RUN dotnet restore "src/Library.Gateway/Library.Gateway.csproj"

COPY . .

WORKDIR /src/src/Library.Gateway
RUN dotnet build "Library.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Library.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Library.Gateway.dll"]