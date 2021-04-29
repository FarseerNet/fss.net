dotnet publish -c release
cd bin/release/net5.0
docker build -t farseernet/fss:1.0.0-beta.2 --network=host .