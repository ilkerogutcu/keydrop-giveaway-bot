FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build-env

WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o Out

FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy

RUN apt-get update && apt-get install -y wget
RUN wget -q https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
RUN apt-get install -y ./google-chrome-stable_current_amd64.deb

WORKDIR /App
COPY --from=build-env /App/Out .
ENTRYPOINT ["dotnet", "KeyDropGiveawayBot.dll"]