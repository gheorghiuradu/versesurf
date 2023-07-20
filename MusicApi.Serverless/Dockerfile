#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src/MusicApi.Serverless
COPY ["MusicApi.Serverless/MusicApi.Serverless.csproj", ""]
COPY ["SharedDomain/SharedDomain.csproj", "../SharedDomain/"]
COPY ["MusicEventDbApi/MusicEventDbApi.csproj", "../MusicEventDbApi/"]
COPY ["FirestoreExtensions/FirestoreExtensions.csproj", "../FirestoreExtensions/"]
COPY ["GcloudWebApiExtensions/GcloudWebApiExtensions.csproj", "../GcloudWebApiExtensions/"]
COPY ["MusicDbApi/MusicDbApi.csproj", "../MusicDbApi/"]
COPY ["PlayFabService/PlayFabService.csproj", "../PlayFabService/"]
RUN dotnet restore "./MusicApi.Serverless.csproj"

COPY ["MusicApi.Serverless/.", "."]
COPY ["SharedDomain/.", "../SharedDomain/"]
COPY ["MusicEventDbApi/.", "../MusicEventDbApi/"]
COPY ["FirestoreExtensions/.", "../FirestoreExtensions/"]
COPY ["GcloudWebApiExtensions/.", "../GcloudWebApiExtensions/"]
COPY ["MusicDbApi/.", "../MusicDbApi/"]
COPY ["PlayFabService/.", "../PlayFabService/"]
WORKDIR "/src/MusicApi.Serverless/."
RUN dotnet build "MusicApi.Serverless.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicApi.Serverless.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicApi.Serverless.dll"]