using System;

namespace MyCompany.Messages.Commands
{
    public interface ICommand
    {
        DateTime DateSent { get; set; }
    }
}
