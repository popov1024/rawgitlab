ARG DOCKERREGISTRY=mcr.microsoft.com
ARG DOCKERMMIRROR=
FROM ${DOCKERREGISTRY}/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN apk add --no-cache clang lld musl-dev
COPY *.sln ./
COPY src/*.csproj ./src/
ARG NUGET=https://api.nuget.org/v3/index.json
RUN dotnet restore ./src/rawgitlab.csproj --source ${NUGET}
COPY src/ ./src/
WORKDIR /src/src
RUN dotnet publish -c Release -o /app/publish /p:StripSymbols=true

FROM ${DOCKERMMIRROR}alpine AS runtime
WORKDIR /app
EXPOSE 8080
RUN apk add --no-cache ca-certificates && \
    adduser -D -u 1000 appuser
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish/rawgitlab .
RUN chown appuser:appuser /app/rawgitlab
USER appuser
ENTRYPOINT ["/app/rawgitlab"]
