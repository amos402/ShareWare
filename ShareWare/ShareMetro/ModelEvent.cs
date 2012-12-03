using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMetro
{
    public class ModelEvent : EventArgs
    {
        public ModelEvent(ModelEventType type)
        {
            Type = type;
        }
        public ModelEventType Type { get; set; }
        public Exception ModelException { get; set; }
        public string FailedMessage { get; set; }
    }

    public enum ModelEventType
    {
        Exception,
        Meesage
    }
}
