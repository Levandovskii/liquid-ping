namespace LiquidPing.Core.Models;

public enum ProbeKind
{
    Icmp,
    Tcp,
    Http
}

public enum ProbeError
{
    None,
    Timeout,
    DnsFailure,
    Refused,
    Unreachable,
    HttpError
}

public enum HostStatus
{
    Pending,
    Available,
    Unstable,
    Unavailable
}

public enum ThemeMode
{
    System,
    Light,
    Dark
}
