using System;

namespace Capabilities
{
    public enum ETickGroup : int
    {
        NetworkEarly,
        Input,
        Gameplay,
        AfterGameplay,
        NetworkLate,
    }
}