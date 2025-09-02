// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.Properties;

/// <summary>
/// Provides commonly used icons for the WebSearchShortcut application
/// </summary>
internal static class Icons
{
    /// <summary>
    /// Extension logo icon
    /// </summary>
    public static IconInfo Logo { get; } = IconHelpers.FromRelativePath("Assets\\Search.png");

    /// <summary>
    /// "Add Shortcut" icon
    /// </summary>
    public static IconInfo AddShortcut { get; } = IconHelpers.FromRelativePath("Assets\\SearchAdd.png");

    /// <summary>
    /// "Edit Shortcut" icon
    /// </summary>
    public static IconInfo EditShortcut { get; } = new("\uE70F");

    /// <summary>
    /// Default fallback icon for links
    /// </summary>
    public static IconInfo Link { get; } = new("🔗");

    /// <summary>
    /// Edit icon (pencil)
    /// </summary>
    public static IconInfo Edit { get; } = new("\uE70F");

    /// <summary>
    /// Delete icon (trash can)
    /// </summary>
    public static IconInfo Delete { get; } = new("\uE74D");

    /// <summary>
    /// Homepage icon
    /// </summary>
    public static IconInfo Home { get; } = new("\uE80F");

    /// <summary>
    /// Search icon
    /// </summary>
    public static IconInfo Search { get; } = new("\uE721");

    /// <summary> 
    /// History icon 
    /// </summary>
    public static IconInfo History { get; } = new("\uE81C");

    /// <summary>
    /// Delete History icon
    /// </summary>
    public static readonly IconInfo DeleteHistory = new("\uE894");
}
