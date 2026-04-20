using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace RatingStandardizer.Emby;

/// <summary>
/// The main Emby plugin class.
/// </summary>
public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">The application paths.</param>
    /// <param name="xmlSerializer">The xml serializer.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Rating Standardizer";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("e2688ed7-4bb2-4bba-9e0d-6ccfd1f2d8f3");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        var type = GetType();

        return
        [
            new PluginPageInfo
            {
                Name = "ratingstandardizer-v6",
                DisplayName = Name,
                EmbeddedResourcePath = type.Namespace + ".Configuration.configPage.html",
                EnableInMainMenu = true,
                MenuSection = "server",
                MenuIcon = "lock"
            },
            new PluginPageInfo
            {
                Name = "ratingstandardizerjs-v6",
                EmbeddedResourcePath = type.Namespace + ".Configuration.configPage.js"
            }
        ];
    }
}
