namespace FSS.Service.Request
{
    public class GetTaskListRequest
    {
        public int GroupId   { get; set; }
        public int PageSize  { get; set; }
        public int PageIndex { get; set; }
    }
}