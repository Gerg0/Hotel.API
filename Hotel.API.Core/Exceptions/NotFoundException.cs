using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.API.Core.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object key): base($"{name} ({key}) was not found")
        {
            
        }
    }
}