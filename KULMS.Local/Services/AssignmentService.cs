using System;
using System.Collections.Generic;
using KULMS.Local.Models;

namespace KULMS.Local.Services;

public class AssignmentService
{
    public static AssignmentService AssignmentManager = new();

    private AssignmentService()
    {
    }

    public async IAsyncEnumerable<AssignmentModel> Filter(IAsyncEnumerable<AssignmentModel> assignments, Func<AssignmentModel, bool> key)
    {
        await foreach (var a in assignments)
        {
            if (key(a))
            {
                yield return a;
            }
        }
    }
}
