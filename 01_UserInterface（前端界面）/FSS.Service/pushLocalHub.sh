docker kill fss
docker rm fss
docker rmi farseernet/fss:latest

dotnet publish -c release
cd bin/release/net6.0/publish
docker build -t farseernet/fss:latest --network=host .

docker run -d --name fss -p 888:888 \
--network net --network-alias fss farseernet/fss:latest \
--restart=always