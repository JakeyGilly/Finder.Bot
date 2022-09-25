#!/bin/bash
set -e
run_cmd="dotnet run"
until dotnet ef migrations add initbot; do
>&2 echo "PostgreSQL is migrating database"
sleep 1
done
until dotnet ef database update; do
>&2 echo "PostgreSQL is updating database"
sleep 1
done
>&2 echo "PostgreSQL is up - executing command"
exec $run_cmd
