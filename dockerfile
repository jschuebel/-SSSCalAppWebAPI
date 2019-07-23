FROM mcr.microsoft.com/windows/servercore:ltsc2016-amd64

MAINTAINER Jim Schuebel <schuebelsoft@yahoo.com>

WORKDIR /app

SHELL ["powershell", "-command"]


COPY bin\Release\netcoreapp2.1\publish .


# Expose ports

EXPOSE 5000

#ENV ASPNETCORE_URLS http://*:5000

#HEALTHCHECK --interval=30s --timeout=3s --retries=1 CMD curl --silent --fail http://localhost:5000/hc || exit 1


# Start

ENTRYPOINT ["dotnet", "SSSCalAppWebAPI.dll"]
