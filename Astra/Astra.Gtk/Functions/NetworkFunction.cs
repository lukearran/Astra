using GLib;

namespace Astra.Gtk.Functions;

public static class NetworkFunction
{
    public static async Task<Bytes?> GetDataInBytesAsync(string url)
    {
        using var client = new HttpClient();

        // Create the request
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Send the request, and get the body
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Convert the response to GDK bytes type
        var byteArray = await response.Content.ReadAsByteArrayAsync();

        return Bytes.New(byteArray);
    }
}