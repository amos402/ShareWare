using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareMetro
{
    public class ModelEventArgs : EventArgs
    {
        public ModelEventArgs(ModelEventType type)
        {
            Type = type;
        }
        public ModelEventType Type { get; set; }
        public Exception ModelException { get; set; }
        public string FailedMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum ModelEventType
    {
        Exception,
        ErrorMessage,
        ConnectMeesage
    }
}
