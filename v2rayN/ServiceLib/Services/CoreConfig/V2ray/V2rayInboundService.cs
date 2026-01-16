namespace ServiceLib.Services.CoreConfig;

public partial class CoreConfigV2rayService
{
    private async Task<int> GenInbounds(V2rayConfig v2rayConfig, ProfileItem? node = null)
    {
        try
        {
            var listen = "0.0.0.0";
            v2rayConfig.inbounds = [];

            // Check if node has a custom local port configured
            int? customPort = node?.CustomLocalPort;
            var inbound = GetInbound(_config.Inbound.First(), EInboundProtocol.socks, true, customPort);
            v2rayConfig.inbounds.Add(inbound);

            if (_config.Inbound.First().SecondLocalPortEnabled)
            {
                var inbound2 = GetInbound(_config.Inbound.First(), EInboundProtocol.socks2, true, customPort);
                v2rayConfig.inbounds.Add(inbound2);
            }

            if (_config.Inbound.First().AllowLANConn)
            {
                if (_config.Inbound.First().NewPort4LAN)
                {
                    var inbound3 = GetInbound(_config.Inbound.First(), EInboundProtocol.socks3, true, customPort);
                    inbound3.listen = listen;
                    v2rayConfig.inbounds.Add(inbound3);

                    //auth
                    if (_config.Inbound.First().User.IsNotEmpty() && _config.Inbound.First().Pass.IsNotEmpty())
                    {
                        inbound3.settings.auth = "password";
                        inbound3.settings.accounts = new List<AccountsItem4Ray> { new AccountsItem4Ray() { user = _config.Inbound.First().User, pass = _config.Inbound.First().Pass } };
                    }
                }
                else
                {
                    inbound.listen = listen;
                }
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        return await Task.FromResult(0);
    }

    private Inbounds4Ray GetInbound(InItem inItem, EInboundProtocol protocol, bool bSocks, int? customPort = null)
    {
        var result = EmbedUtils.GetEmbedText(Global.V2raySampleInbound);
        if (result.IsNullOrEmpty())
        {
            return new();
        }

        var inbound = JsonUtils.Deserialize<Inbounds4Ray>(result);
        if (inbound == null)
        {
            return new();
        }
        inbound.tag = protocol.ToString();

        // Use custom port if specified and valid, otherwise use default
        int basePort = (customPort.HasValue && customPort.Value > 0) ? customPort.Value : inItem.LocalPort;
        inbound.port = basePort + (int)protocol;

        inbound.protocol = EInboundProtocol.mixed.ToString();
        inbound.settings.udp = inItem.UdpEnabled;
        inbound.sniffing.enabled = inItem.SniffingEnabled;
        inbound.sniffing.destOverride = inItem.DestOverride;
        inbound.sniffing.routeOnly = inItem.RouteOnly;

        return inbound;
    }
}
