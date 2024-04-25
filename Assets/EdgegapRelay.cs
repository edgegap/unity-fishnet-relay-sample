using System.Collections.Generic;

public class Users
{
    public string ip { get; set; }
}

public class RootObject
{
    public List<Users> users { get; set; }
}
public class Client
{
    public ushort port { get; set; }
    public string protocol { get; set; }
    public string link { get; set; }
}

public class Ports
{
    public Server server { get; set; }
    public Client client { get; set; }
}

public class Relay
{
    public string ip { get; set; }
    public string host { get; set; }
    public Ports ports { get; set; }
}

public class ApiResponse
{
    public string session_id { get; set; }
    public uint? authorization_token { get; set; }
    public string status { get; set; }
    public bool ready { get; set; }
    public bool linked { get; set; }
    public object? error { get; set; }
    public List<SessionUser>? session_users { get; set; }
    public Relay relay { get; set; }
    public object? webhook_url { get; set; }
}

public class Server
{
    public ushort port { get; set; }
    public string protocol { get; set; }
    public string link { get; set; }
}

public class SessionUser
{
    public string ip_address { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public uint? authorization_token { get; set; }
}

public class PublicIP
{
    public string public_ip { get; set; }
}