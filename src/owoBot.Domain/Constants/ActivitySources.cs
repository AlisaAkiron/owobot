﻿using System.Diagnostics;

namespace owoBot.Domain.Constants;

public static class ActivitySources
{
    public static readonly ActivitySource AppActivitySource = new("owoBot.App");

    public static readonly ActivitySource CommandActivitySource = new("owoBot.Application.Command");

    public static readonly ActivitySource CommandAutocompletionActivitySource = new("owoBot.Application.Command.Autocompletion");

    public static IEnumerable<string> AllActivitySources
    {
        get
        {
            yield return AppActivitySource.Name;
            yield return CommandActivitySource.Name;
            yield return CommandAutocompletionActivitySource.Name;
        }
    }
}
