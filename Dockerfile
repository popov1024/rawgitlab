# Build stage - Native AOT requires Alpine with musl
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Install native AOT dependencies
RUN apk add --no-cache clang lld musl-dev

# Copy solution and project files
COPY *.sln ./
COPY src/*.csproj ./src/
RUN dotnet restore ./src/rawgitlab.csproj

# Copy source code
COPY src/ ./src/
WORKDIR /src/src

# Publish as Native AOT
RUN dotnet publish -c Release -o /app/publish /p:StripSymbols=true

# Final stage - minimal Alpine runtime (scratch has no libc)
FROM alpine:3.21 AS runtime
WORKDIR /app
EXPOSE 8080

# Install ca-certificates for HTTPS and set ASPNETCORE_URLS
RUN apk add --no-cache ca-certificates && \
    adduser -D -u 1000 appuser

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish/rawgitlab .
RUN chown appuser:appuser /app/rawgitlab

USER appuser

ENTRYPOINT ["/app/rawgitlab"]
