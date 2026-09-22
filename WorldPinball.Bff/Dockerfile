# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["WorldPinball.Bff/WorldPinball.Bff.csproj", "WorldPinball.Bff/"]
RUN dotnet restore "WorldPinball.Bff/WorldPinball.Bff.csproj"

# Copy full source and publish release artifact
COPY . .
WORKDIR "/src/WorldPinball.Bff"
RUN dotnet publish "WorldPinball.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal hardened runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "WorldPinball.Bff.dll"]