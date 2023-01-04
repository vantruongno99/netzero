using System;
using System.Collections.Generic;
using System.Text;

namespace MongoAccess
{
    public class MongoDbSet
    {        
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }      
    }
}
