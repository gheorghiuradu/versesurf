FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
EXPOSE 80

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
COPY . /src
RUN dotnet restore -a "$TARGETARCH" "/src/MusicServer/MusicServer.csproj" && \
    dotnet publish "/src/MusicServer/MusicServer.csproj" --no-restore -a "$TARGETARCH" -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
RUN useradd --uid 1001452020 -l -g root versesurfer && \
    mkdir /home/versesurfer && chown -R versesurfer:root /app /home/versesurfer && chmod -R g=u /app && \
    apt update && DEBIAN_FRONTEND=noninteractive apt install -y libcap2-bin && apt upgrade -y && apt clean && rm -rf /var/lib/apt/lists/* && \
    setcap CAP_NET_BIND_SERVICE=+eip /usr/share/dotnet/dotnet
WORKDIR /app
USER versesurfer
ENTRYPOINT ["dotnet", "MusicServer.dll"]