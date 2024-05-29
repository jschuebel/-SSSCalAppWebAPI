#FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
#FROM mcr.microsoft.com/businesscentral/sandbox:ltsc2019
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
MAINTAINER Jim Schuebel <schuebelsoft@yahoo.com>

# Install ASP.NET Core Runtime
ENV Audience__Secret Y2F0Y2hlciUyMHdvbmclMjBsb3ZlJTIwLm5ldA==
ENV ConnectionStrings__DefaultConnection Data Source=192.168.50.3;Integrated Security=False;User ID=jschuebel;Password=weeb;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False


WORKDIR /app
COPY publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "SSSCalAppWebAPI.dll"]
