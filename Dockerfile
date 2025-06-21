FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

## copiar csproj e sln para cache melhor
COPY *.sln ./
COPY FlutterStart.Apresentation/*.csproj ./FlutterStart.Apresentation/
COPY FlutterStart.Application/*.csproj ./FlutterStart.Application/
COPY FlutterStart.Domain/*.csproj ./FlutterStart.Domain/
COPY FlutterStart.Infrastructure/*.csproj ./FlutterStart.Infrastructure/

RUN dotnet restore ./FlutterStart.Apresentation/FlutterStart.Apresentation.csproj

COPY . .
RUN dotnet publish ./FlutterStart.Apresentation/FlutterStart.Apresentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Incluir só se precisa de yt-dlp/ffmpeg
RUN apt-get update && apt-get install -y python3 python3-pip ffmpeg && \
    pip3 install yt-dlp && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/downloads && chmod 777 /app/downloads

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "FlutterStart.Apresentation.dll"]