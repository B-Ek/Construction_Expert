using System.Collections.Generic;
using System;
namespace Construction_Expert.Models
{
    public record CategoryNode(Guid Id, string Code, string Name, IReadOnlyList<CategoryNode> Children);

}


