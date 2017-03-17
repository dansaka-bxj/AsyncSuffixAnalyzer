# AsyncSuffixAnalyzer
Hungarian notation enforcement for ``Task``-producing methods. Two diagnostics with corresponding code fixes enforces that methods returning ``Task`` posses name ending with ``Async``.

These rules follows [Microsoft recommendation](https://msdn.microsoft.com/en-us/library/mt674882.aspx). Read [No Async Suffix](https://docs.particular.net/nservicebus/upgrades/5to6/async-suffix) before deciding that the convention suits you.

[![NuGet Status](http://img.shields.io/badge/nuget-1.0.6285-green.svg)](https://www.nuget.org/packages/AsyncSuffixAnalyzer)
