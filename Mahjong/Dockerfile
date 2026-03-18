# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create the data folder for SQLite and set permissions
RUN mkdir -p /app/data && chmod 777 /app/data

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mahjong/Mahjong.csproj", "Mahjong/"]
RUN dotnet restore "./Mahjong/Mahjong.csproj"
COPY . .
WORKDIR "/src/Mahjong"
ARG GIT_COMMIT_HASH=unknown
ARG GIT_COMMIT_COUNT=0
ARG GIT_COMMIT_DATE=unknown
RUN dotnet build "./Mahjong.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:GitCommitHash=$GIT_COMMIT_HASH /p:GitCommitCount=$GIT_COMMIT_COUNT /p:GitCommitDate="$GIT_COMMIT_DATE"

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG GIT_COMMIT_HASH=unknown
ARG GIT_COMMIT_COUNT=0
ARG GIT_COMMIT_DATE=unknown
RUN dotnet publish "./Mahjong.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:GitCommitHash=$GIT_COMMIT_HASH /p:GitCommitCount=$GIT_COMMIT_COUNT /p:GitCommitDate="$GIT_COMMIT_DATE"

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mahjong.dll"]