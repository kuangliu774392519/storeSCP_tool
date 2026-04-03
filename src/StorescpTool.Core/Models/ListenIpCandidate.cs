namespace StorescpTool.Core.Models;

public sealed record ListenIpCandidate(
    string Address,
    bool HasGateway,
    bool IsPreferredAdapterType,
    bool IsVirtualAdapter,
    long Speed,
    string InterfaceName);
