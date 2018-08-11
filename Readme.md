# WinDirStat.Net (Name subject to change)
A .NET/WPF implementation of [WinDirStat](https://windirstat.net/). Name needs to be changed due to it conflicting with the original WinDirStat's webpage url.

![Preview](https://i.imgur.com/HbW3gz1.png)

### Pros

* WPF allows for more advanced flashy features
* Includes NTFS MFT parsing for extremely fast reads of the primary or non-Windows-owned drives (Requires elevated priviledges)
* Smoother UI, less freezing
* Faster scanning than original WinDirStat.

### Cons

* C# overhead means higher memory usage, especially with 64-bit mode.
* Slightly slower scanning than [altWinDirStat](https://github.com/ariccio/altWinDirStat).
* Scrollbars are too damn tiny beacuse of WPF (Will fix with style override)

### Priority Todo List

* Replace use of Windows Icons with Fugue Icon pack
