FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:~/.dotnet/tools/"
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh