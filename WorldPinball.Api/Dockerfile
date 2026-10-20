# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies to leverage Docker layer caching
COPY ["WorldPinball.Api/WorldPinball.Api.csproj", "WorldPinball.Api/"]
COPY ["WorldPinball.Application/WorldPinball.Application.csproj", "WorldPinball.Application/"]
COPY ["WorldPinball.Domain/WorldPinball.Domain.csproj", "WorldPinball.Domain/"]
COPY ["WorldPinball.Infrastructure/WorldPinball.Infrastructure.csproj", "WorldPinball.Infrastructure/"]
COPY ["Aspire/ServiceDefaults/ServiceDefaults.csproj", "Aspire/ServiceDefaults/"]
RUN dotnet restore "WorldPinball.Api/WorldPinball.Api.csproj"

# Copy full source and publish release artifact
COPY . .
WORKDIR "/src/WorldPinball.Api"
RUN dotnet publish "WorldPinball.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal hardened runtime image with globalization & ICU support
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WorldPinball.Api.dll"]
