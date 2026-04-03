using System.Net;
using System.Net.Sockets;
using StorescpTool.Core.Models;

namespace StorescpTool.Core.Utilities;

public static class ListenIpSelector
{
    public static string Select(string configuredIp, IReadOnlyList<string> availableIps)
    {
        if (!string.IsNullOrWhiteSpace(configuredIp))
        {
            return configuredIp;
        }

        return availableIps.FirstOrDefault() ?? string.Empty;
    }

    public static IReadOnlyList<string> OrderCandidates(IEnumerable<ListenIpCandidate> candidates)
    {
        return candidates
            .Where(candidate => IsSupportedIpv4(candidate.Address))
            .GroupBy(candidate => candidate.Address, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(GetScore)
                .ThenByDescending(candidate => candidate.Speed)
                .ThenBy(candidate => candidate.InterfaceName, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderByDescending(GetScore)
            .ThenByDescending(candidate => candidate.Speed)
            .ThenBy(candidate => candidate.InterfaceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.Address, StringComparer.OrdinalIgnoreCase)
            .Select(candidate => candidate.Address)
            .ToList();
    }

    private static int GetScore(ListenIpCandidate candidate)
    {
        var score = 0;

        if (candidate.HasGateway)
        {
            score += 100;
        }

        if (candidate.IsPreferredAdapterType)
        {
            score += 30;
        }

        if (candidate.IsVirtualAdapter)
        {
            score -= 50;
        }

        return score;
    }

    private static bool IsSupportedIpv4(string address)
    {
        return IPAddress.TryParse(address, out var ipAddress) &&
               ipAddress.AddressFamily == AddressFamily.InterNetwork &&
               !address.StartsWith("169.254.", StringComparison.Ordinal);
    }
}
