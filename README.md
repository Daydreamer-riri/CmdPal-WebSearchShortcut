# CmdPal-WebSearchShortcut

[![Mentioned in Awesome PowerToys Run Plugins](https://awesome.re/mentioned-badge.svg)](https://github.com/hlaueriksson/awesome-powertoys-run-plugins)

[![Microsoft](https://get.microsoft.com/images/en-us%20dark.svg)](https://apps.microsoft.com/detail/9p9hchbgbrf4)

This is a simple [PowerToys CmdPal](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/overview) plugin for quickly selecting a specific search engine to perform searches via keyword prefixes.

> If you are looking for the Powertoys Run plugin, please check: [WebSearchShortcut run plugin](./README-Run.md)

## Preview

![Preview of the plugin in action](./ScreenShots/preview-cmdpal.gif)

## Installation

### MSStore

Click the badge below to go to the store page:

[![Microsoft](https://get.microsoft.com/images/en-us%20dark.svg)](https://apps.microsoft.com/detail/9p9hchbgbrf4)

### WinGet

`winget install Riri.WebSearchShortcut`

### MSIX Files

You can download the MSIX files from the [Releases](https://github.com/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/releases) page on GitHub. Choose the appropriate file for your architecture (**x64**(more common) or **ARM**) and install it manually.

## Configuration

- Access the command shown in the image:

![Open the configuration"](./ScreenShots/cmdpal-add.png)

- Complete the form's content and save it.

## Configuration fields

### `Url`

The URL template for performing the search. Use `%s` as a placeholder for the search query. If the URL does not contain `%s`, the URL will be opened directly when you press enter.

```json
{
  "Google": {
    "Url": "https://www.google.com/search?q=%s"
  }
}
```

> More example: https://github.com/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/blob/main/README-Run.md#example

### `ReplaceWhitespace`

With `ReplaceWhitespace`, you can specify which character(s) to replace a space with when performing a search. This is useful for some websites, such as Wikipedia, which don't use plus signs ("+") to separate words in the URL.

| Value          | Result             |
|----------------|--------------------|
| `""` (Default) | `Example+search`   |
| `"+"`          | `Example+search`   |
| `"%20"`        | `Example%20search` |
| `"-"`          | `Example-search`   |
| `"_"`          | `Example_search`   |

> **Note**: The character defined in `ReplaceWhitespace` will be used **literally** as the separator without further encoding. While some modern browsers may automatically encode invalid characters, users are responsible for ensuring their chosen character is valid and compatible with the target URL template. For more information on valid URL characters, please refer to [percent-encoding](https://en.wikipedia.org/wiki/Percent-encoding).

### `SuggestionProvider`

Used to set the search suggestion provider.

**The currently supported search suggestion providers are:**

- `Google`
- `Bing`
- `npm`
- `CanIUse`

Planned:

- [x] `DuckDuckGo`
- [ ] `Yandex`
- [ ] `Baidu`
- [x] `Youtube`
- [x] `Wiki`

PRs welcome!

> You can also set a Provider to another search engine.
> For example:
>
> ![add-suggestions](./ScreenShots/cmdpal-add-suggestions.png)

## Credits

- This project can only be completed under the guidance of [this article](https://conductofcode.io/post/creating-custom-powertoys-run-plugins/). Thanks to @hlaueriksson for his great work.

- The search suggestion feature of this project is based on the relevant implementation of [FlowLauncher](https://github.com/Flow-Launcher/Flow.Launcher?tab=readme-ov-file#web-searches--urls), thanks @Flow-Launcher!

## Thanks

Thank you to [@thatgaypigeon](https://github.com/thatgaypigeon) for writing the excellent documentation!
Thank you to [@Tortoise](https://github.com/Testudinidae) for making many very useful refactorings and improvements to this project!

## License

[MIT](./LICENSE) License © 2023 [Riri](https://github.com/Daydreamer-riri)
