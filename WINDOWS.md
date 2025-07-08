# Sunflower GUI Windows Client
I made a bigger focus on this part than on the console version of the client, because I am more friendly with the window interface, to be honest. However, the main support for output and processing results is there and there.

In a window variation of the client, the main window is the recently opened files window. If the table is empty, you can open the file from the menu `File->Read Executable` or catch the process `File->Catch Win32 Process`. These actions will add your selected file to the table.

<img src="assets/mainwnd.png">

The table of recent files is contained in the directory of the registry  `... Registry\recent.json`, and can be easily supplemented by your notes.

When the file is selected, the uploader starts to check for plugins in the home directory of the `...\Plugins` and marks the ready-to-use files in a special properties window.
From the properties window, to know the results of extensions work, you should call the editor, because there will be written from memory the expected information. 

<img src="assets/mdbook.png">

So, you can save results to file `File->Save document` or ~manually~ clean a WebView cache (see main window menu) `Application->Clear Editor Cache`.

All extensions in list (see property window) are ready to use (compatible with sunflower loader). You can manually enable or disable what you want to see it in results document.

> [!WARNING]
> Disabling plugins in list directly changes plugins`Model` part,  but doesn't unload them from RAM. (unfortunately).

Second bad thing is themes. Application supports themes if runs under Microsoft Windows 10 and above, but

> [!WARNING]
> You can't change theme manually, because I gave up to fix Application resources dictionary.

Also Accent colors you can't change too. App uses system setup...

