#docker name 前缀
n='fss-grpc'
#dockerFile应用目录
appPath='/root/nfs/git/FarseerSchedulerService/01_UserInterface（前端界面）/FSS.GrpcService'
v=`date +%Y_%m_%d_%H_%M_%S`
dockerHub='10.102.171.169:5000'

git -C /root/nfs/git/Farseer.Net pull
git -C ${appPath} pull

cd ${appPath}
dotnet publish -nowarn:msb3202,nu1503,cs1591 -c Release -o /home/release/${n}
cd /home/release/${n}
docker build -t ${dockerHub}:${n}-${v} --network=host .

#docker push ${dockerHub}:${n}-${v} # jenkins与k8s在同一台机器，不需删除
#docker rmi ${dockerHub}:${n}-${v} # jenkins与k8s在同一台机器，不需删除
kubectl set image deployment/${n} ${n}=${dockerHub}:${n}-${v}