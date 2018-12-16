using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smidigprosjekt.Models
{
  public class Lobby
  {
    public int Id { get; set; }
    public ConcurrentDictionary<int, User> Members { get; set; }
    public List<Message> Messages {get;set;}
  }
  

}
