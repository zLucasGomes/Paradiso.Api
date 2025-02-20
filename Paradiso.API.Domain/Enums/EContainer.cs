using System.ComponentModel;
using System;

namespace Paradiso.API.Domain.Enums;

public enum EContainer
{
    Movies,
    Photos,
    Sounds,
    Scripts
}

public static class Container
{
    public static string GetName(this EContainer value) => value.ToString().ToLower();
}
