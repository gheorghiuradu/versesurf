#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src/MusicServer
COPY ["MusicServer/MusicServer.csproj", ""]
COPY ["SharedDomain/SharedDomain.csproj", "../SharedDomain/"]
COPY ["MusicStorageClient/MusicStorageClient.csproj", "../MusicStorageClient/"]
COPY ["MusicDbApi/MusicDbApi.csproj", "../MusicDbApi/"]
COPY ["MusicEventDbApi/MusicEventDbApi.csproj", "../MusicEventDbApi/"]
COPY ["GamePlayingContext/GamePlaying/GamePlaying.csproj", "../GamePlayingContext/GamePlaying/"]
COPY ["GamePlayingContext/GamePlaying.Repositories/GamePlaying.Repositories.csproj", "../GamePlayingContext/GamePlaying.Repositories/"]
COPY ["SteamWebApi.Client/SteamWebApi.Client.csproj", "../SteamWebApi.Client/"]
RUN dotnet restore "./MusicServer.csproj"

COPY ["MusicServer/.", "."]
COPY ["SharedDomain/.", "../SharedDomain/"]
COPY ["MusicStorageClient/.", "../MusicStorageClient/"]
COPY ["MusicDbApi/.", "../MusicDbApi/"]
COPY ["MusicEventDbApi/.", "../MusicEventDbApi/"]
COPY ["GamePlayingContext/GamePlaying/.", "../GamePlayingContext/GamePlaying/"]
COPY ["GamePlayingContext/GamePlaying.Repositories/.", "../GamePlayingContext/GamePlaying.Repositories/"]
COPY ["SteamWebApi.Client/.", "../SteamWebApi.Client/"]
WORKDIR "/src/MusicServer/."
RUN dotnet build "MusicServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicServer.dll"]