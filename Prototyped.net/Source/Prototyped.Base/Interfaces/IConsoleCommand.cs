﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototyped.Base.Interfaces
{
    public interface IConsoleCommand
    {
        string HelpDesc { get; }

        void RunCommand(string[] args);
        void ShowHelpText(string[] args);
    }
}
