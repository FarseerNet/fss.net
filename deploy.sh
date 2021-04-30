dockerName=${dockerHub}/${JOB_NAME}:-${BUILD_NUMBER}
appPath='/root/nfs/git/FarseerSchedulerService/01_UserInterface（前端界面）/FSS.GrpcService'
echo "打包镜像地址：${dockerName}"
echo "GIT项目地址：${appPath}"

cd ${appPath}
dotnet publish -c Release -o /home/release/${JOB_NAME}
cd /home/release/${JOB_NAME}
docker build -t ${dockerName} --network=host .

docker push ${dockerName}
docker rmi ${dockerName}
kubectl set image deployment/${JOB_NAME} ${JOB_NAME}=${dockerName}