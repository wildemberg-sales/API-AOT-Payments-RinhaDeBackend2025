FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

RUN apt-get update && apt-get install -y --no-install-recommends clang zlib1g-dev

WORKDIR /source

COPY *.csproj .
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o /app/aot-output -r linux-x64 /p:PublishAot=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0

RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/aot-output .

COPY init.sql .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["/app/entrypoint.sh"]

CMD ["./ApiPaymets"]