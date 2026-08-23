FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Server/GuessNumber.Domain/GuessNumber.Domain.csproj Server/GuessNumber.Domain/
COPY Server/GuessNumber.Application/GuessNumber.Application.csproj Server/GuessNumber.Application/
COPY Server/GuessNumber.Infrastructure/GuessNumber.Infrastructure.csproj Server/GuessNumber.Infrastructure/
COPY Server/GuessNumber.API/GuessNumber.API.csproj Server/GuessNumber.API/

RUN dotnet restore Server/GuessNumber.API/GuessNumber.API.csproj

COPY Server/ ./Server/

RUN dotnet publish Server/GuessNumber.API/GuessNumber.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GuessNumber.API.dll"]
