FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY src/XLoad/bin/Release/netcoreapp3.1/publish/ XLoad/

COPY examples/http.json XLoad/config.json 

COPY src/XLoad.Http/bin/Release/netcoreapp3.1/publish/ XLoad/plugins/XLoad.Http 

WORKDIR /XLoad
ENTRYPOINT ["dotnet", "XLoad.dll"]
