using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
  public enum MessageType { Global, User, Vote }
  public class Message
  {
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageType Type { get; set; }
    public string User { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
  }
}
