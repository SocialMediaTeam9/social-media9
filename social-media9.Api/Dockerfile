FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY social-media9.Api.csproj ./
RUN dotnet restore social-media9.Api.csproj
COPY . .
RUN dotnet build social-media9.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish social-media9.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "social-media9.dll"]
