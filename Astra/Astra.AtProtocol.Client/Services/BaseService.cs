using FishyFlip;

namespace Astra.AtProtocol.Client.Services;

public class BaseService(ATProtocol protocol)
{
    protected readonly ATProtocol Protocol = protocol;
}