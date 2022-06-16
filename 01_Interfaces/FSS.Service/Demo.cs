using FS.Core.Abstract.AspNetCore;

namespace FSS.Service;

[UseApi(Area = "test")]
public class Demo
{
    [Api("/demo2/hello2", HttpMethod.GET, "请求成功啦！")]
    public string Hello(int id)
    {
        return $"Hello:{id}";
    }
}