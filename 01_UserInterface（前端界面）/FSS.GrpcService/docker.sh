ver='1.0.0'
dotnet publish -c release
cd bin/release/net5.0
docker build -t farseernet/fss:${ver} --network=host .
docker push farseernet/fss:${ver}

docker tag farseernet/fss:${ver} farseernet/fss:latest
docker push farseernet/fss:latest