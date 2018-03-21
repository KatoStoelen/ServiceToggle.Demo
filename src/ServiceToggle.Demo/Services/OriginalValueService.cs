namespace ServiceToggle.Demo.Services
{
    public class OriginalValueService : IValueService
    {
        public string GetValue()
        {
            return "Wrong value";
        }
    }
}