using SanjeshP.Payeh.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Services.DataInitializer
{
    public interface IDataInitializer : IScopedDependency
    {
        void InitializeData();
    }
}
