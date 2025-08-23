using System.Text;
using System.Text.Json;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Credentials;
using DBus.Services.Secrets;
using Microsoft.Extensions.Logging;

namespace Astra.Provider.Credentials.DBusSecretService;

public class SecretServiceProvider(ILogger<ICredentialProvider> logger) : ICredentialProvider
{
    private const string SecretLabel = "Astra Credential";
    
    private readonly Dictionary<string, string> _attributes = new()
    {
        { "name", "io.github.lukearran.astra.credential" }
    };
    
    private SecretService? _secretService;
    
    public async Task<AtCredential?> GetCredential()
    {
        logger.LogDebug("{MethodName}: Getting Credential...", nameof(GetCredential));
        
        var collection = await GetCollection();

        if (collection == null)
        {
            logger.LogError("Secret Service Provider failed to retrieve credential " +
                             "because the collection was not available for use.");
            return null;
        }
        
        var items = await collection.SearchItemsAsync(_attributes);
        var item = items.SingleOrDefault();

        if (item == null)
        {
            logger.LogDebug("Secret Service Provider failed to retrieve credential. " +
                             "Has it been created?");
            
            return null;
        }
        
        logger.LogDebug(
            "Secret Service Provider retrieved credential which was created at {CreatedAt}. Attempting to parse into valid object...",
            await item.GetCreatedAsync());
        
        var secret = await item.GetSecretAsync();
        var secretString = Encoding.UTF8.GetString(secret);
        var credential = JsonSerializer.Deserialize<AtCredential>(secretString);

        return credential;
    }

    public async Task<bool> SetCredential(AtCredential credential)
    {
        logger.LogDebug("{MethodName}: Setting Credential...", nameof(SetCredential));
        
        var collection = await GetCollection();

        if (collection == null)
        {
            logger.LogError("Secret Service Provider failed to create credential " +
                             "because the collection was not available for use.");
            
            return false;
        }
        
        var secretBytes = Encoding.UTF8.GetBytes(credential.ToJson());
        const string contentType = "application/json; charset=utf8";

        var created = await collection.CreateItemAsync(
            label: SecretLabel,
            lookupAttributes: _attributes,
            secret: secretBytes,
            contentType: contentType,
            replace: true);

        if (created is null)
        {
            logger.LogError("Secret Service Provider failed to create credential due to unknown error.");
        }

        return created is not null;
    }

    private async Task<Collection?> GetCollection()
    {
        var secretService = await GetSecretService();
        
        var defaultCollection = await secretService.GetDefaultCollectionAsync();

        if (defaultCollection == null)
        {
            logger.LogError("Could not retrieve default collection, exiting");
            return null;
        }
        
        var defaultCollectionProperties = await defaultCollection.GetAllPropertiesAsync();

        if (defaultCollectionProperties.Locked)
        {
            await defaultCollection.UnlockAsync();
        }

        return defaultCollection;
    }

    private async Task<SecretService> GetSecretService()
    {
        if (_secretService != null)
        {
            return _secretService;
        }
        
        _secretService = await SecretService.ConnectAsync(EncryptionType.Dh);
        
        return _secretService;
    }
}