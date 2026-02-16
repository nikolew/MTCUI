using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MTCUI.Services
{
    public interface IInitializableWindow
    {
        Task InitializeAsync(DispatcherQueue dispatcher, object o);
    }

}
