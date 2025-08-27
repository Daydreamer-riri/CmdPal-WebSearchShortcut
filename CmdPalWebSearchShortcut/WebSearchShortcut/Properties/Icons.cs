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
    /// <summary> Default fallback icon for links </summary>
    public static readonly IconInfo Link = new("🔗");

    /// <summary> Edit icon (pencil) </summary>
    public static readonly IconInfo Edit = new("\uE70F");

    /// <summary> Delete icon (trash can) </summary>
    public static readonly IconInfo Delete = new("\uE74D");

    /// <summary> Homepage icon </summary>
    public static readonly IconInfo Home = new("\uE80F");

    /// <summary> Search icon </summary>
    public static readonly IconInfo Search = new("\uE721");

    /// <summary> History icon </summary>
    public static readonly IconInfo History = new("\uE81C");

    /// <summary> Delete History icon </summary>
    public static readonly IconInfo DeleteHistory = new("\uE894");
}
