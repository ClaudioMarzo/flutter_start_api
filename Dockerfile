FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

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

# Instalar dependências do sistema
RUN apt-get update && \
    apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg \
    curl \
    wget \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Criar diretório de downloads com permissões adequadas
RUN mkdir -p /app/downloads && \
    chmod 777 /app/downloads && \
    chown -R app:app /app/downloads || true

COPY --from=build /app/publish .

# Copiar executáveis do yt-dlp
COPY yt-dlp_linux /app/yt-dlp_linux
RUN chmod +x /app/yt-dlp_linux

COPY yt-dlp_windows.exe /app/yt-dlp_windows.exe
RUN chmod +x /app/yt-dlp_windows.exe

# Copiar script de configuração de cookies
COPY setup-render-cookies.sh /app/setup-render-cookies.sh
RUN chmod +x /app/setup-render-cookies.sh

# Copiar cookies se disponível (opcional para desenvolvimento)
# COPY cookies.txt /app/cookies.txt

# Configurar variáveis de ambiente
ENV DOTNET_RUNNING_IN_CONTAINER=TRUE
ENV ASPNETCORE_URLS=http://+:8080
ENV TZ=UTC
ENV PYTHONUNBUFFERED=1

EXPOSE 8080

# Script de inicialização que configura cookies e inicia a aplicação
RUN echo '#!/bin/bash\n/app/setup-render-cookies.sh\nexec dotnet FlutterStart.Apresentation.dll' > /app/start.sh && \
    chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]