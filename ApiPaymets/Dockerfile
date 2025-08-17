# Dockerfile (localizado dentro da pasta ApiPaymets)

# Estágio 1: Compilação (Build Stage) - Usando a imagem Debian padrão
# Esta imagem contém a toolchain C/C++ completa (gcc, glibc) que o AOT precisa.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Instala as dependências de compilação necessárias no ambiente Debian.
RUN apt-get update && apt-get install -y --no-install-recommends clang zlib1g-dev

WORKDIR /source

# Otimização de cache
COPY *.csproj .
RUN dotnet restore

# Copia o resto do código-fonte
COPY . .

# Publique a aplicação AOT. O resultado será um executável Linux compatível com glibc.
RUN dotnet publish -c Release -o /app/aot-output -r linux-x64 /p:PublishAot=true


# Estágio 2: Imagem Final (Final Stage) - Usando a imagem ASP.NET baseada em Debian
# Esta imagem é otimizada para produção e é compatível com o executável que acabamos de criar.
# Ela é maior que a 'alpine', mas garante 100% de compatibilidade de bibliotecas.
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/aot-output .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# !!! SUBSTITUA "ApiPaymets" PELO NOME REAL DO SEU EXECUTÁVEL !!!
ENTRYPOINT ["./ApiPaymets"]