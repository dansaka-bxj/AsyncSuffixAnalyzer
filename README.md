# AsyncSuffixAnalyzer
[AsyncSuffixAnalyzer](https://github.com/lukas-lansky/AsyncSuffixAnalyzer)をフォークし、Unity向けのカスタマイズを加えたリポジトリです。

## ビルド方法
以下コマンドを実行することでビルドできます。

````
nuget restore AsyncSuffixAnalyzer.sln
msbuild /p:Configuration=Release AsyncSuffixAnalyzer.sln
````

以下、原文
-----------------------------------------------------------------------------------------------------

# AsyncSuffixAnalyzer
Hungarian notation enforcement for ``Task``-producing methods. Two diagnostics with corresponding code fixes enforces that methods returning ``Task`` posses name ending with ``Async``.

These rules follows [Microsoft recommendation](https://msdn.microsoft.com/en-us/library/mt674882.aspx). Read [No Async Suffix](https://docs.particular.net/nservicebus/upgrades/5to6/async-suffix) before deciding that the convention suits you.

[![NuGet Status](http://img.shields.io/badge/nuget-1.0.6285-green.svg)](https://www.nuget.org/packages/AsyncSuffixAnalyzer)
