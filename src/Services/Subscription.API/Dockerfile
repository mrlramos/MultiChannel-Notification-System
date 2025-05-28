# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj e restaurar dependências
COPY ["Subscription.API.csproj", "./"]
RUN dotnet restore

# Copiar o resto do código e build
COPY . .
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Definir ponto de entrada
ENTRYPOINT ["dotnet", "Subscription.API.dll"] 