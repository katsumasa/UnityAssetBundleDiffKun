# UnityAssetBundleDiffKun

![GitHub package.json version](https://img.shields.io/github/package-json/v/katsumasa/UnityAssetBundleDiffKun)

Editor extension that could let you compare the differences of AssetBundle.

## Summary

Have you ever experienced the following:
- AssetBundle has been updated even though no one has made any changes.
- Want to know the difference between the AssetBundle that was built last time and the AssetBundle that's going to be built this time.

This is an Editor extension to compare the contents of the AssetBundle and see the changes that were made.

![UnityAssetBundleDiffKun](https://user-images.githubusercontent.com/29646672/137438506-ee9cc60e-b5e5-4d2d-9700-a0ea6c23c17e.png)

## How to install

Needs UnityAsseBundleDiffKunは別途[UnityCommandLineTools](https://github.com/katsumasa/UnityCommandLineTools)
Please obtain UnityCommandLineTools together with this repository.

```
git clone https://github.com/katsumasa/UnityAssetBundleDiffKun.git
git clone https://github.com/katsumasa/UnityCommandLineTools.git
```

## How to use

`Start EditorWindow from `Window > UnityAssetBundleDiffKun`
Specify the two AssetBundles you want to compare in the left and right screen of Object Fields respectively.

- [Object Field] Set the AssetBundle to be compared in each left and right side of [Object Field]
- [WebExtract] Expands AssetBundle
- [Popup選択フィールド] The list of Assets included in the AssetBundle can be displayed and selected.
- [Bin2Text] Select the Asset included in the expanded AssetBundle from Popup, convert it to text format and displays it.
- [Diff] Displays the difference between Assets displayed in the left and right views. You can change the display format by specifying the option when executing diff from the pulldown menu for selection.
- [Verify] Compare the Assets that include the AssetBundles set on the left and right in order.

## Reference URL

https://support.unity3d.com/hc/en-us/articles/217123266-How-do-I-determine-what-is-in-my-Scene-bundle-
