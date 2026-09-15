ARG DOCKERREGISTRY=mcr.microsoft.com
ARG DOCKERMMIRROR=
FROM ${DOCKERREGISTRY}/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
# Native AOT for musl (Alpine) requires the LLVM toolchain
RUN apk add --no-cache clang lld build-base
COPY *.sln ./
COPY src/*.csproj ./src/
ARG NUGET=https://api.nuget.org/v3/index.json
RUN dotnet restore ./src/rawgitlab.csproj --source ${NUGET}
COPY src/ ./src/
WORKDIR /src/src
RUN dotnet publish --no-restore -c Release -o /app/publish /p:StripSymbols=true

FROM ${DOCKERMMIRROR}alpine:3.24.1 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
RUN apk add --no-cache ca-certificates curl && \
    adduser -D -u 1000 appuser
COPY --from=build /app/publish/rawgitlab .
RUN chown appuser:appuser /app/rawgitlab
USER appuser
ENTRYPOINT ["/app/rawgitlab"]
