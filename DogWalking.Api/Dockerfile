FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj files and restore
COPY DogWalking.Domain/DogWalking.Domain.csproj DogWalking.Domain/
COPY DogWalking.Application/DogWalking.Application.csproj DogWalking.Application/
COPY DogWalking.Infrastructure/DogWalking.Infrastructure.csproj DogWalking.Infrastructure/
COPY DogWalking.Api/DogWalking.Api.csproj DogWalking.Api/
RUN dotnet restore DogWalking.Api/DogWalking.Api.csproj

# Copy everything and build
COPY . .
RUN dotnet publish DogWalking.Api/DogWalking.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DogWalking.Api.dll"]
