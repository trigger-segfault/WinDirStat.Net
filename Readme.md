# WinDirStat.Net (Name subject to change)

<!--[![Latest Release](https://img.shields.io/github/release/trigger-death/WinDirStat.Net.svg?style=flat&label=version)](https://github.com/trigger-death/WinDirStat.Net/releases/latest)
[![Latest Release Date](https://img.shields.io/github/release-date-pre/trigger-death/WinDirStat.Net.svg?style=flat&label=released)](https://github.com/trigger-death/WinDirStat.Net/releases/latest)-->
[![Total Downloads](https://img.shields.io/github/downloads/trigger-death/WinDirStat.Net/total.svg?style=flat)](https://github.com/trigger-death/WinDirStat.Net/releases)
[![Creation Date](https://img.shields.io/badge/created-august%202018-A642FF.svg?style=flat)](https://github.com/trigger-death/WinDirStat.Net/commit/3aa1fde1cfb165ea8bc119df2944ede41f063179)
[![Discord](https://img.shields.io/discord/436949335947870238.svg?style=flat&logo=discord&label=chat&colorB=7389DC&link=https://discord.gg/vB7jUbY)](https://discord.gg/vB7jUbY)

A .NET/WPF implementation of [WinDirStat](https://windirstat.net/). Name needs to be changed due to it conflicting with the original WinDirStat's webpage url.

![Preview](https://i.imgur.com/BaFZZVI.png)

### Credits

* NTFS reading done by [NTFS Reader](https://sourceforge.net/projects/ntfsreader/) library with GNU 2.1 license.
* File Tree View modified from [ICSharpCode.TreeView](https://github.com/icsharpcode/SharpDevelop/tree/master/src/Libraries/SharpTreeView/ICSharpCode.TreeView).
* Currently using some Windows 10 icons. (MUST FIX SOON)

### Pros

* WPF allows for more advanced flashy features
* Includes NTFS MFT parsing for extremely fast reads of the primary or non-Windows-owned drives (Requires elevated priviledges)
* Smoother, more responsive UI, less freezing.
* New blur effect when scaling treemap render.
* Faster scanning than original WinDirStat.

### Cons

* C# overhead means higher memory usage, especially with 64-bit mode.
* Slightly slower scanning than [altWinDirStat](https://github.com/ariccio/altWinDirStat).
* Scrollbars are too damn tiny beacuse of WPF (Will fix with style override)
* Folders with *just* files still use the `<Files>` subfolder. (Will try to fix)

### Priority Todo List

* Replace use of Windows Icons with Fugue Icon pack
