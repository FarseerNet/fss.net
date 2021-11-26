# 添加基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY . ./

#设置时区
RUN cp /usr/share/zoneinfo/GMT /etc/localtime
RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai    /etc/localtime

#设置Docker容器对外暴露的端口
#EXPOSE 80

#容器中使用 ["dotnet","系统启动的dll"] 来运行应用程序
ENTRYPOINT ["dotnet", "FSS.Service.dll"]