FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
#fix for https://github.com/GoogleCloudPlatform/google-cloud-dotnet-powerpack/issues/22
RUN apt update && apt install -y libc-dev 

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src/BackOffice.Web
COPY ["BackOffice.Web/BackOffice.Web.csproj", ""]
COPY ["GcloudWebApiExtensions/GcloudWebApiExtensions.csproj", "../GcloudWebApiExtensions/"]
COPY ["TaskService/TaskService.csproj", "../TaskService/"]
COPY ["ArtManager/ArtManager.csproj", "../ArtManager/"]
COPY ["BMIRepertoireClient/LicensingService.csproj", "../BMIRepertoireClient/"]
COPY ["MusicDbApi/MusicDbApi.csproj", "../MusicDbApi/"]
COPY ["MusicStorageClient/MusicStorageClient.csproj", "../MusicStorageClient/"]
COPY ["MusicEventDbApi/MusicEventDbApi.csproj", "../MusicEventDbApi/"]
COPY ["MusixClient/MusixClient.csproj", "../MusixClient/"]
COPY ["SpotifyApiService/SpotifyApiService.csproj", "../SpotifyApiService/"]
COPY ["PlayFabService/PlayFabService.csproj", "../PlayFabService/"]
COPY ["FirestoreExtensions/FirestoreExtensions.csproj", "../FirestoreExtensions/"]
COPY ["AudioProcessing/AudioProcessing.csproj", "../AudioProcessing/"]
COPY ["SharedDomain/SharedDomain.csproj", "../SharedDomain/"]
RUN dotnet restore "./BackOffice.Web.csproj"

COPY ["BackOffice.Web/.", "."]
COPY ["GcloudWebApiExtensions/.", "../GcloudWebApiExtensions/"]
COPY ["TaskService/.", "../TaskService/"]
COPY ["ArtManager/.", "../ArtManager/"]
COPY ["BMIRepertoireClient/.", "../BMIRepertoireClient/"]
COPY ["MusicDbApi/.", "../MusicDbApi/"]
COPY ["MusicStorageClient/.", "../MusicStorageClient/"]
COPY ["MusicEventDbApi/.", "../MusicEventDbApi/"]
COPY ["MusixClient/.", "../MusixClient/"]
COPY ["SpotifyApiService/.", "../SpotifyApiService/"]
COPY ["PlayFabService/.", "../PlayFabService/"]
COPY ["FirestoreExtensions/.", "../FirestoreExtensions/"]
COPY ["SharedDomain/.", "../SharedDomain/"]
COPY ["AudioProcessing/.", "../AudioProcessing/"]
RUN dotnet build  "./BackOffice.Web.csproj" -c Release -o /app/build

FROM build as publish
RUN dotnet publish "./BackOffice.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "BackOffice.Web.dll" ]