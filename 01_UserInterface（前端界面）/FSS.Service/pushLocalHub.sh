docker kill fss
docker rm fss
docker rmi farseernet/fss:latest

dotnet publish -c release
cd bin/release/net6.0
docker build -t farseernet/fss:latest --network=host .

docker run -d --name fss -p 888:888 \
-e Redis__0__Server=docker.for.mac.host.internal:6379,dbIndex=13,connecttimeout=600000,synctimeout=10000,responsetimeout=10000 \
-e Database__Items__0__Server=docker.for.mac.host.internal \
-e ElasticSearch__0__Server=http://docker.for.mac.host.internal:9200 \
-e ElasticSearch__1__Server=http://docker.for.mac.host.internal:9200 \
--network=net farseernet/fss:latest \
 --restart=always