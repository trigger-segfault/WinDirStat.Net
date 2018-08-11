# WinDirStat.Net (Name subject to change)
A .NET/WPF implementation of [WinDirStat](https://windirstat.net/). Name needs to be changed due to it conflicting with the original WinDirStat's webpage url.

![Preview](https://i.imgur.com/HbW3gz1.png)

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

### Priority Todo List

* Replace use of Windows Icons with Fugue Icon pack
