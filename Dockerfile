FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
RUN ["dotnet", "tool", "install", "--global", "dotnet-ef"]
ENV PATH="${PATH}:~/.dotnet/tools/"
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh