# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /workspace
COPY --from=buildingblocks / /workspace/fcg-dotnet-building-blocks
COPY . /workspace/fcg-notifications-service
WORKDIR /workspace/fcg-notifications-service
RUN --mount=type=secret,id=nuget_config,target=/root/.nuget/NuGet/NuGet.Config,required=true \
    dotnet restore FiapCloudGames.Notifications.slnx -p:UseLocalBuildingBlocks=true --configfile /root/.nuget/NuGet/NuGet.Config
RUN dotnet publish src/FiapCloudGames.Notifications.Api/FiapCloudGames.Notifications.Api.csproj \
    -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /app/api
RUN dotnet publish src/FiapCloudGames.Notifications.Migrations/FiapCloudGames.Notifications.Migrations.csproj \
    -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /app/migrations
RUN dotnet publish src/FiapCloudGames.Notifications.Worker/FiapCloudGames.Notifications.Worker.csproj \
    -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /app/worker

FROM runtime AS migrations
COPY --from=build /app/migrations .
ENTRYPOINT ["dotnet", "FiapCloudGames.Notifications.Migrations.dll"]

FROM runtime AS worker
COPY --from=build /app/worker .
ENTRYPOINT ["dotnet", "FiapCloudGames.Notifications.Worker.dll"]

FROM runtime AS final
COPY --from=build /app/api .
ENTRYPOINT ["dotnet", "FiapCloudGames.Notifications.Api.dll"]
