using System;
using System.Collections.Generic;
using System.Text;

namespace Checkme.BL.Abstract.Exceptions
{
    public class ItemExistsException:Exception
    {
        public ItemExistsException(string message):base(message)
        {

        }
    }
}
