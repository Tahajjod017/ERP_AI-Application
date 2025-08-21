

using Newtonsoft.Json;

namespace GCTL.Core.Helpers.Jsonserialize
{
    public static class JsonSettings
    {
        public static readonly JsonSerializerSettings IgnoreReferenceLoop = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }
    
    }