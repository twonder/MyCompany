using System;

namespace MyCompany.Messages.Events
{
    public interface IEvent
    {
        DateTime DateOccurred { get; set; }
    }
}
