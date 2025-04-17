# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install fontconfig and download Arial
RUN apk add --no-cache fontconfig ttf-dejavu && \
    mkdir -p /usr/share/fonts/microsoft && \
    wget -q -O /usr/share/fonts/microsoft/arial.ttf https://github.com/matomo-org/travis-scripts/raw/master/fonts/Arial.ttf && \
    fc-cache -fv

# Base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project file and restore dependencies (leveraging Docker cache)
COPY ["VillaVerkenerAPI.csproj", "./"]
RUN dotnet restore "./VillaVerkenerAPI.csproj"

# Copy the rest of the source code and build the application
COPY . .
RUN dotnet build "./VillaVerkenerAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "./VillaVerkenerAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VillaVerkenerAPI.dll"]
