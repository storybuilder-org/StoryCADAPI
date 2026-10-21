using System.Text.Json;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StoryCADLib.Services;
using StoryCADLib.Services.Store;

namespace StoryCADMcp;

/// <summary>
/// Collaborator #291, design section 5.5. The installed StoryCAD app keeps the Worker token in
/// its Preferences.json. This class copies the four sign-in keys into the server's own
/// preferences and lets the activation service pick them up, so a run charges the same
/// subscriber the app does. It never writes the app's file, and it never lets the token out in a
/// tool result.
/// </summary>
internal static class CollaboratorSignIn
{
    internal const string MissingMessage =
        "No Collaborator sign-in found. Sign in to Collaborator in StoryCAD, then try again.";

    internal const string ExpiredMessage = "Open StoryCAD once to refresh the Collaborator sign-in.";

    private static string? _knownToken;

    /// <summary>Tests point this at a file of their own.</summary>
    internal static string? PreferencesPathOverride { get; set; }

    /// <summary>
    /// Reads the four keys from the installed app again and activates. Returns an error message,
    /// or null when Collaborator is signed in. Call it before each run so a token the app
    /// refreshed since the last run is used.
    /// </summary>
    internal static async Task<string?> EnsureAsync()
    {
        var path = PreferencesPathOverride ?? FindInstalledPreferences();
        if (path is null || !File.Exists(path))
            return MissingMessage;

        string guid, jwt, activationPath;
        DateTime expiry;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;
            guid = ReadString(root, "StoreUserGuid");
            jwt = ReadString(root, "StoreActivationJwt");
            activationPath = ReadString(root, "StoreActivationPath");
            expiry = root.TryGetProperty("StoreActivationJwtExpiry", out var e) && e.ValueKind == JsonValueKind.String
                     && e.TryGetDateTime(out var parsed)
                ? parsed
                : DateTime.MinValue;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return MissingMessage;
        }

        if (string.IsNullOrEmpty(jwt))
            return MissingMessage;

        _knownToken = jwt;
        var model = Ioc.Default.GetRequiredService<PreferenceService>().Model;
        model.StoreUserGuid = guid;
        model.StoreActivationJwt = jwt;
        model.StoreActivationJwtExpiry = expiry;
        model.StoreActivationPath = activationPath;

        var activation = Ioc.Default.GetRequiredService<IStoreActivationService>();
        await activation.InitializeAsync();
        return activation.State == ActivationState.Active ? null : ExpiredMessage;
    }

    /// <summary>Removes the token from a string that is about to leave the server.</summary>
    internal static string Scrub(string text)
    {
        var token = _knownToken;
        return string.IsNullOrEmpty(token) ? text : text.Replace(token, "[token removed]");
    }

    private static string ReadString(JsonElement root, string key) =>
        root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    /// <summary>
    /// The installed app's Preferences.json: the packaged Windows app, an unpackaged Windows
    /// build, or the macOS app.
    /// </summary>
    internal static string? FindInstalledPreferences()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var packages = Path.Combine(local, "Packages");
        if (Directory.Exists(packages))
        {
            var packaged = Directory.GetDirectories(packages, "34432StoryBuilder.StoryBuilder_*")
                .Select(d => Path.Combine(d, "RoamingState", "StoryCAD", "Preferences.json"))
                .FirstOrDefault(File.Exists);
            if (packaged is not null)
                return packaged;
        }

        var windows = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StoryCAD", "Preferences.json");
        if (File.Exists(windows))
            return windows;

        var mac = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library", "Application Support", "StoryCAD", "Preferences.json");
        return File.Exists(mac) ? mac : null;
    }
}
