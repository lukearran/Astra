<p align="center">
  <img width="160" src="./Astra/Astra.Gtk/Resources/astra-icon-colour.svg">
</p>
<h1 align="center">Astra</h1>
<h3 align="center">A GTK 4 Bluesky client for Linux</h3>

<p align="center">
  <img width="500" src="./docs/images/feed_screenshot.png">
</p>

> [!WARNING]  
> This app is still early in development and is **not** ready for daily use.

## 🛣️ Roadmap

- [ ] Login
    - [ ] Credential storage
    - [ ] Change provider
- [ ] Timeline
    - [X] Avatar
    - [X] Text
    - [X] Image
        - [X] One image
        - [X] Group of images
        - [ ] Full screen image viewer
    - [ ] Video
    - [X] Embedded Link
    - [ ] Likes Indicator
        - [ ] Submit like
    - [ ] Repost Indicator
        - [ ] Submit repost
    - [ ] Quote post
    - [ ] View Replies
    - [X] Continuous Load
    - [ ] Change feeds/algorithms
    - [ ] Create a post
    - [ ] Responsive layout
    - [ ] Searching
    - [ ] Notifications
    - [ ] Messages

## 🛠️ Building from source

#### 1. Prerequisites

- [.NET SDK 8.0](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet9&pivots=os-linux-ubuntu-2404)

#### 2. Clone the repository

```bash
git clone https://github.com/lukearran/Astra
```

This will create a local copy of the repository.

#### 3. Build the project

To build Astra for development, run the following command:

``` bash
dotnet run --project Astra/Astra.Gtk
```

## 🙋 Contributing

Want to contribute to this project? Let me know with an [issue](https://github.com/lukearran/Astra/issues) that communicates your intent to create a [pull request](https://github.com/lukearran/Astra/pulls).

## ⚖️ License

This project is licensed under the `MIT` license. See the `LICENSE.md` file for in the root directory for details.

## 😇 Acknowledgements

Astra uses the following libraries:
- [FishyFlip](https://github.com/drasticactions/FishyFlip)
- [Gir.Core](https://github.com/gircore/)
- [Blueprint](https://jwestman.pages.gitlab.gnome.org/blueprint-compiler/)