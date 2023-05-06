FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dotnet-base

ENV DOCKERIZE_VERSION="v0.6.1" \
  # Tini for Docker
  TINI_VERSION="v0.19.0"


# === Builder image ===
FROM dotnet-base AS builder

#System dependencies
RUN apt update && apt upgrade -y && apt install --no-install-recommends -y \
  curl \
  wget \
  ffmpeg

# Dockerize
RUN wget "https://github.com/jwilder/dockerize/releases/download/${DOCKERIZE_VERSION}/dockerize-linux-amd64-${DOCKERIZE_VERSION}.tar.gz" \
  && tar -C /usr/local/bin -xzf "dockerize-linux-amd64-${DOCKERIZE_VERSION}.tar.gz" \
  && rm "dockerize-linux-amd64-${DOCKERIZE_VERSION}.tar.gz" \
  && dockerize --version

# Tini
RUN wget -O /usr/local/bin/tini "https://github.com/krallin/tini/releases/download/${TINI_VERSION}/tini" \
  && chmod +x /usr/local/bin/tini \
  && tini --version


# === Base image ===
FROM dotnet-base AS base

COPY --from=builder /usr/local/bin/dockerize /usr/local/bin/dockerize
COPY --from=builder /usr/local/bin/tini /usr/local/bin/tini
COPY --from=builder /usr/bin/ffmpeg /usr/bin/ffmpeg
COPY --from=builder /usr/lib/*-linux-gnu/* /usr/lib/
COPY --from=builder /lib/*-linux-gnu/* /lib/

COPY ./scripts/entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# === Development image ===
FROM base AS dev

WORKDIR /SkyTube
COPY ./SkyTube.csproj .

RUN dotnet restore

ENTRYPOINT [ "tini", "--", "/docker-entrypoint.sh" ]

CMD [ "dotnet", "watch", "run" ]


# === Build production image ===
FROM dev AS build

COPY . ./

RUN dotnet build -c Release -o /SkyTube/out


# === Production image ===
FROM base AS prod

# Setup Permissions
RUN groupadd -r dotnet && useradd -m -d /dotnet -r -g dotnet dotnet \
  && chown dotnet:dotnet -R /SkyTube

COPY --from=build /SkyTube/out /SkyTube

USER dotnet

ENTRYPOINT [ "tini", "--", "/docker-entrypoint.sh" ]

CMD [ "dotnet", "SkyTube.dll" ]
